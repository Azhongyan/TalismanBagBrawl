#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public enum BuildSandboxConfigPanelTab
    {
        Overview,
        MapRule,
        EnemyProblem,
        BossProblem,
        WeaknessWindow,
        DropBias,
        BuildReadiness,
        DevChapter,
        Simulation,
        Validation
    }

    public sealed class BuildSandboxConfigPanelWindow : EditorWindow
    {
        public const string MenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/Data/[Manual Only] BuildSandbox Config Panel 01";

        public const string WindowTitle = "BuildSandbox Config Panel 01";

        private static readonly string[] TabLabels =
        {
            "Overview / 总览",
            "MapRule / 地图规则",
            "EnemyProblem / 敌人题目",
            "BossProblem / Boss 锁",
            "WeaknessWindow / 弱点窗口",
            "DropBias / 定向倾向",
            "BuildReadiness / 破题准备度",
            "DevChapter / 章节预留",
            "Simulation / 模拟预留",
            "Validation / 校验"
        };

        private readonly Dictionary<string, bool> foldouts = new(StringComparer.Ordinal);
        private BuildProblemSeedDataset dataset;
        private IReadOnlyList<BuildSandboxValidationReport> validationReports;
        private BuildSandboxConfigPanelTab selectedTab;
        private Vector2 scroll;
        private string searchText = string.Empty;
        private bool showInfo;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            BuildSandboxConfigPanelWindow window =
                GetWindow<BuildSandboxConfigPanelWindow>(WindowTitle);
            window.minSize = new Vector2(980f, 640f);
            window.Reload();
            window.Show();
            window.Focus();
        }

        private void OnEnable()
        {
            Reload();
        }

        private void OnGUI()
        {
            DrawToolbar();

            int previousTab = (int)selectedTab;
            selectedTab = (BuildSandboxConfigPanelTab)GUILayout.SelectionGrid(
                previousTab,
                TabLabels,
                5,
                EditorStyles.toolbarButton,
                GUILayout.Height(46f));
            if ((int)selectedTab != previousTab)
            {
                scroll = Vector2.zero;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Search / 搜索", GUILayout.Width(90f));
                searchText = EditorGUILayout.TextField(searchText ?? string.Empty);
            }

            scroll = EditorGUILayout.BeginScrollView(
                scroll,
                false,
                true,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            switch (selectedTab)
            {
                case BuildSandboxConfigPanelTab.Overview:
                    DrawOverview();
                    break;
                case BuildSandboxConfigPanelTab.MapRule:
                    DrawMapRules();
                    break;
                case BuildSandboxConfigPanelTab.EnemyProblem:
                    DrawEnemyProblems();
                    break;
                case BuildSandboxConfigPanelTab.BossProblem:
                    DrawBossProblems();
                    break;
                case BuildSandboxConfigPanelTab.WeaknessWindow:
                    DrawWeaknessWindows();
                    break;
                case BuildSandboxConfigPanelTab.DropBias:
                    DrawDropBiases();
                    break;
                case BuildSandboxConfigPanelTab.BuildReadiness:
                    DrawBuildReadiness();
                    break;
                case BuildSandboxConfigPanelTab.DevChapter:
                    DrawDevChapterPlaceholder();
                    break;
                case BuildSandboxConfigPanelTab.Simulation:
                    DrawSimulationPlaceholder();
                    break;
                default:
                    DrawValidation();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(WindowTitle, EditorStyles.boldLabel, GUILayout.Width(260f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reload / 重新载入", EditorStyles.toolbarButton, GUILayout.Width(150f)))
                {
                    Reload();
                }

                if (GUILayout.Button("Validate / 校验", EditorStyles.toolbarButton, GUILayout.Width(130f)))
                {
                    RunValidation();
                    selectedTab = BuildSandboxConfigPanelTab.Validation;
                    scroll = Vector2.zero;
                }

                if (GUILayout.Button("Export Reports / 导出报告", EditorStyles.toolbarButton, GUILayout.Width(170f)))
                {
                    ExportReports();
                    selectedTab = BuildSandboxConfigPanelTab.Validation;
                    scroll = Vector2.zero;
                }
            }

            EditorGUILayout.HelpBox(
                "V0.4 BuildSandbox devOnly read-only panel. It reads static seed data, validates isolation, and writes V0.4 reports only. It does not edit V0.2 / V0.3 product config, UI, save, reward, drop, Boss, or numeric data.",
                MessageType.Info);
        }

        private void DrawOverview()
        {
            DrawHeader("Overview / 总览", "Read-only static seed overview for V0.4 BuildSandbox Phase 2.");
            DrawReadOnlyRow("Data Source", "BuildProblemSeedDataset.CreateDefault()");
            DrawReadOnlyRow("Package", dataset.packageName);
            DrawReadOnlyRow("Dataset devOnly", dataset.devOnly.ToString());
            DrawReadOnlyRow("Dataset isEnabled", dataset.isEnabled.ToString());
            DrawReadOnlyRow("Menu Path", MenuPath);
            DrawReadOnlyRow("Window Title", WindowTitle);
            DrawReadOnlyRow("Tabs", TabLabels.Length.ToString());
            EditorGUILayout.Space(6f);
            DrawCountGrid();
            EditorGUILayout.Space(6f);
            DrawHeader("FeatureFlags", "All BuildSandbox FeatureFlags must remain false.");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                DrawReadOnlyRow(flag.Key, flag.DefaultValue.ToString());
            }
        }

        private void DrawMapRules()
        {
            DrawHeader("MapRule / 地图规则", "Static devOnly map rules. Read-only in this first panel package.");
            foreach (MapRuleSeed rule in Filtered(dataset.mapRules, rule => rule.mapRuleId, rule => rule.displayName))
            {
                if (!DrawFoldout(rule.mapRuleId, $"{rule.displayName} [{rule.mapRuleId}]"))
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawReadOnlyRow("Description", rule.description);
                    DrawReadOnlyRow("Affected Tags", Join(rule.affectedTags));
                    DrawReadOnlyRow("Buff Tags", Join(rule.buffTags));
                    DrawReadOnlyRow("Debuff Tags", Join(rule.debuffTags));
                    DrawReadOnlyRow("Placement Modifier", rule.placementModifier.ToString());
                    DrawReadOnlyRow("Energy Modifier", rule.energyModifier.ToString());
                    DrawReadOnlyRow("Cooldown Modifier", rule.cooldownModifier.ToString());
                    DrawReadOnlyTextArea("Warning", rule.warningText);
                    DrawReadOnlyRow("Enemy Problems", Join(rule.enemyProblemIds));
                    DrawReadOnlyRow("Boss Problems", Join(rule.bossProblemIds));
                    DrawReadOnlyRow("DropBias", rule.dropBiasId);
                    DrawIsolation(rule.devOnly, rule.isEnabled, rule.entersFormalFlow);
                }
            }
        }

        private void DrawEnemyProblems()
        {
            DrawHeader("EnemyProblem / 敌人题目", "Static devOnly enemy problem categories. Read-only in this first panel package.");
            foreach (EnemyProblemSeed problem in Filtered(dataset.enemyProblems, item => item.problemType, item => item.displayName))
            {
                if (!DrawFoldout(problem.problemType, $"{problem.displayName} [{problem.problemType}]"))
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawReadOnlyRow("Pressure Type", problem.pressureType);
                    DrawReadOnlyTextArea("Summary", problem.problemSummary);
                    DrawReadOnlyRow("Hard Solution Tags", Join(problem.hardSolutionTags));
                    DrawReadOnlyRow("Soft Solution Tags", Join(problem.softSolutionTags));
                    DrawReadOnlyRow("Validated Build Tags", Join(problem.validatedBuildTags));
                    DrawReadOnlyRow("Failure Hint", problem.failureHint?.failureHintId);
                    DrawReadOnlyTextArea("Recommended Action", problem.recommendedAction);
                    DrawIsolation(problem.devOnly, problem.isEnabled, problem.entersFormalFlow);
                }
            }
        }

        private void DrawBossProblems()
        {
            DrawHeader("BossProblem / Boss 多钥匙锁", "Static devOnly Boss problem locks. Read-only in this first panel package.");
            foreach (BossProblemSeed boss in Filtered(dataset.bossProblems, item => item.bossProblemId, item => item.displayName))
            {
                if (!DrawFoldout(boss.bossProblemId, $"{boss.displayName} [{boss.bossProblemId}]"))
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawReadOnlyRow("Validation Goal", boss.validationGoal);
                    DrawReadOnlyRow("Required Attributes", Join(boss.requiredProblemAttributes));
                    DrawReadOnlyRow("Minimum Keys Required", boss.minimumKeysRequired.ToString());
                    DrawReadOnlyRow("Key Count", boss.keyRequirements.Count.ToString());
                    DrawReadOnlyRow("Weakness Windows", boss.weaknessWindows.Count.ToString());
                    DrawReadOnlyRow("DropBias", boss.dropBiases.Count.ToString());
                    DrawIsolation(boss.devOnly, boss.isEnabled, boss.entersFormalFlow);
                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField("Keys", EditorStyles.boldLabel);
                    foreach (BossProblemKeySeed key in boss.keyRequirements)
                    {
                        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                        {
                            DrawReadOnlyRow("Key", $"{key.keyCategory} / {key.keyId}");
                            DrawReadOnlyRow("Requirement", key.requirementId);
                            DrawReadOnlyRow("Problem Attribute", key.problemAttribute);
                            DrawReadOnlyRow("Required Score", key.requiredScore.ToString());
                            DrawReadOnlyTextArea("Hint", key.hint);
                            DrawIsolation(key.devOnly, key.isEnabled, key.gatesProductBoss);
                        }
                    }
                }
            }
        }

        private void DrawWeaknessWindows()
        {
            DrawHeader("WeaknessWindow / 弱点窗口", "Derived from BossProblem seeds. Read-only in this first panel package.");
            foreach (WeaknessWindowSeed window in Filtered(dataset.weaknessWindows, item => item.weaknessWindowId, item => item.displayName))
            {
                if (!DrawFoldout(window.weaknessWindowId, $"{window.displayName} [{window.weaknessWindowId}]"))
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawReadOnlyRow("Trigger", window.triggerCondition);
                    DrawReadOnlyRow("Start Second", window.startSecond.ToString("0.##"));
                    DrawReadOnlyRow("Duration Second", window.durationSecond.ToString("0.##"));
                    DrawReadOnlyRow("Exposed Build Tags", Join(window.exposedBuildTags));
                    DrawIsolation(window.devOnly, window.isEnabled, window.entersFormalFlow);
                }
            }
        }

        private void DrawDropBiases()
        {
            DrawHeader("DropBias / 定向倾向", "Report-only devOnly drop bias. It does not edit product drop tables.");
            foreach (DropBiasSeed drop in Filtered(dataset.dropBiases, item => item.dropBiasId, item => item.displayName))
            {
                if (!DrawFoldout(drop.dropBiasId, $"{drop.displayName} [{drop.dropBiasId}]"))
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawReadOnlyRow("Bias Type", drop.biasType);
                    DrawReadOnlyRow("Target Build Tags", Join(drop.targetBuildTags));
                    DrawReadOnlyRow("Target Item Tags", Join(drop.targetItemTags));
                    DrawReadOnlyRow("Target Affix Ids", Join(drop.targetAffixIds));
                    DrawReadOnlyRow("Preview Weight", drop.previewWeight.ToString("0.##"));
                    DrawReadOnlyRow("Reports Only", drop.reportsOnly.ToString());
                    DrawIsolation(drop.devOnly, drop.isEnabled, drop.entersFormalFlow);
                }
            }
        }

        private void DrawBuildReadiness()
        {
            DrawHeader("BuildReadiness / 破题准备度", "Derived readiness coverage from Boss attributes and key requirements.");
            IReadOnlyList<ReadinessSummary> summaries = BuildReadinessSummaries();
            foreach (ReadinessSummary summary in summaries)
            {
                if (!Matches(summary.Attribute, summary.Bosses))
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawReadOnlyRow("Attribute", summary.Attribute);
                    DrawReadOnlyRow("Boss Coverage", summary.Bosses);
                    DrawReadOnlyRow("Key Count", summary.KeyCount.ToString());
                    DrawReadOnlyRow("Reports Only", "True");
                    DrawReadOnlyRow("devOnly / isEnabled", "True / False");
                }
            }
        }

        private void DrawDevChapterPlaceholder()
        {
            DrawHeader("DevChapter / devOnly 章节预留", "Read-only placeholder for later DevChapterProblemRun01.");
            EditorGUILayout.HelpBox(
                "This package does not create 3-10 / 4-10 product chapters. Later packages may read these problem seeds for devOnly preview chapters only.",
                MessageType.Info);
            DrawReadOnlyRow("Status", "Reserved / read-only");
            DrawReadOnlyRow("Formal Chapter Write", "False");
        }

        private void DrawSimulationPlaceholder()
        {
            DrawHeader("Simulation / 模拟结果预留", "Read-only placeholder for later preview context and data panels.");
            EditorGUILayout.HelpBox(
                "This package does not run product combat or modify battle values. Later packages can connect seed data to BuildSandbox simulation reports.",
                MessageType.Info);
            DrawReadOnlyRow("Status", "Reserved / read-only");
            DrawReadOnlyRow("Affects Formal Combat", "False");
        }

        private void DrawValidation()
        {
            DrawHeader("Validation / 校验", "Run panel validation and export V0.4 reports.");
            validationReports ??= BuildSandboxConfigPanelValidator.BuildValidationReports();
            int errors = validationReports.Sum(report => report.ErrorCount);
            int warnings = validationReports.Sum(report => report.WarningCount);
            int infos = validationReports.Sum(report => report.InfoCount);

            EditorGUILayout.LabelField(
                $"Error: {errors}    Warning: {warnings}    Info: {infos}",
                EditorStyles.boldLabel);
            showInfo = EditorGUILayout.ToggleLeft("Show Info / 显示 Info", showInfo);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Validate / 校验", GUILayout.Height(28f), GUILayout.Width(160f)))
                {
                    RunValidation();
                }

                if (GUILayout.Button("Export Reports / 导出报告", GUILayout.Height(28f), GUILayout.Width(190f)))
                {
                    ExportReports();
                }
            }

            foreach (BuildSandboxValidationReport report in validationReports)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    $"{report.Name} - {(report.Passed ? "PASS" : "FAIL")} / E:{report.ErrorCount} W:{report.WarningCount} I:{report.InfoCount}",
                    EditorStyles.boldLabel);
                foreach (BuildSandboxValidationIssue issue in report.Issues)
                {
                    if (!showInfo && issue.Level == BuildSandboxValidationLevel.Info)
                    {
                        continue;
                    }

                    MessageType type = issue.Level switch
                    {
                        BuildSandboxValidationLevel.Error => MessageType.Error,
                        BuildSandboxValidationLevel.Warning => MessageType.Warning,
                        _ => MessageType.Info
                    };
                    EditorGUILayout.HelpBox(issue.ToString(), type);
                }
            }
        }

        private void DrawCountGrid()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawReadOnlyRow("Map Rules", dataset.mapRules.Count.ToString());
                DrawReadOnlyRow("Enemy Problems", dataset.enemyProblems.Count.ToString());
                DrawReadOnlyRow("Boss Problems", dataset.bossProblems.Count.ToString());
                DrawReadOnlyRow("Weakness Windows", dataset.weaknessWindows.Count.ToString());
                DrawReadOnlyRow("DropBias", dataset.dropBiases.Count.ToString());
                DrawReadOnlyRow("Failure Hints", dataset.failureHints.Count.ToString());
            }
        }

        private void RunValidation()
        {
            validationReports = BuildSandboxConfigPanelValidator.BuildValidationReports();
        }

        private void ExportReports()
        {
            validationReports = BuildSandboxConfigPanelValidator.BuildValidationReports();
            string[] paths = BuildSandboxConfigPanelReportWriter.WriteReports(validationReports);
            Debug.Log($"[BuildSandboxConfigPanel01] reports exported: {string.Join(", ", paths)}");
        }

        private void Reload()
        {
            dataset = BuildProblemSeedDataset.CreateDefault();
            validationReports = null;
            foldouts.Clear();
        }

        private IEnumerable<T> Filtered<T>(
            IEnumerable<T> values,
            Func<T, string> idSelector,
            Func<T, string> nameSelector)
        {
            foreach (T value in values ?? Enumerable.Empty<T>())
            {
                if (value == null)
                {
                    continue;
                }

                if (Matches(idSelector(value), nameSelector(value)))
                {
                    yield return value;
                }
            }
        }

        private bool Matches(params string[] values)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string filter = searchText.Trim();
            return values.Any(value =>
                !string.IsNullOrWhiteSpace(value) &&
                value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private bool DrawFoldout(string key, string label)
        {
            foldouts.TryGetValue(key, out bool expanded);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                expanded = EditorGUILayout.Foldout(expanded, label, true);
            }

            foldouts[key] = expanded;
            return expanded;
        }

        private static void DrawHeader(string title, string description)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(description, MessageType.None);
        }

        private static void DrawReadOnlyRow(string label, string value)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(210f));
                EditorGUILayout.TextField(value ?? string.Empty, GUILayout.ExpandWidth(true));
            }
        }

        private static void DrawReadOnlyTextArea(string label, string value)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(value ?? string.Empty, GUILayout.MinHeight(42f));
            }
        }

        private static void DrawIsolation(bool devOnly, bool isEnabled, bool formalFlowFlag)
        {
            DrawReadOnlyRow("devOnly", devOnly.ToString());
            DrawReadOnlyRow("isEnabled", isEnabled.ToString());
            DrawReadOnlyRow("Formal Flow Flag", formalFlowFlag.ToString());
        }

        private IReadOnlyList<ReadinessSummary> BuildReadinessSummaries()
        {
            Dictionary<string, ReadinessSummary> summaries = new(StringComparer.Ordinal);
            foreach (BossProblemSeed boss in dataset.bossProblems)
            {
                foreach (string attribute in boss.requiredProblemAttributes ?? Enumerable.Empty<string>())
                {
                    AddReadiness(summaries, attribute, boss.displayName, 0);
                }

                foreach (BossProblemKeySeed key in boss.keyRequirements ?? Enumerable.Empty<BossProblemKeySeed>())
                {
                    AddReadiness(summaries, key.problemAttribute, boss.displayName, 1);
                }
            }

            return summaries.Values
                .OrderBy(summary => summary.Attribute, StringComparer.Ordinal)
                .ToList();
        }

        private static void AddReadiness(
            IDictionary<string, ReadinessSummary> summaries,
            string attribute,
            string bossName,
            int keyCount)
        {
            if (string.IsNullOrWhiteSpace(attribute))
            {
                return;
            }

            if (!summaries.TryGetValue(attribute, out ReadinessSummary summary))
            {
                summary = new ReadinessSummary(attribute);
                summaries.Add(attribute, summary);
            }

            summary.AddBoss(bossName);
            summary.KeyCount += keyCount;
        }

        private static string Join(IEnumerable<string> values)
        {
            return string.Join("; ", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private sealed class ReadinessSummary
        {
            private readonly SortedSet<string> bosses = new(StringComparer.Ordinal);

            public ReadinessSummary(string attribute)
            {
                Attribute = attribute;
            }

            public string Attribute { get; }
            public int KeyCount { get; set; }
            public string Bosses => string.Join("; ", bosses);

            public void AddBoss(string bossName)
            {
                if (!string.IsNullOrWhiteSpace(bossName))
                {
                    bosses.Add(bossName);
                }
            }
        }
    }
}
#endif
