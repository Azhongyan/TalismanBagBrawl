#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public sealed class BuildTuningDataPanelPreviewWindow : EditorWindow
    {
        public const string MenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/Data/[Manual Only] Build Tuning Data Panel Preview 01";

        public const string WindowTitle = "Build Tuning Data Panel Preview 01";

        private readonly Dictionary<string, bool> foldouts = new(StringComparer.Ordinal);
        private BuildTuningDataPanelPreview preview;
        private IReadOnlyList<BuildSandboxValidationReport> validationReports;
        private Vector2 scroll;
        private string searchText = string.Empty;
        private bool showMaskedOnly;
        private bool showValidationInfo;

        [MenuItem(MenuPath)]
        public static void Open()
        {
            BuildTuningDataPanelPreviewWindow window =
                GetWindow<BuildTuningDataPanelPreviewWindow>(WindowTitle);
            window.minSize = new Vector2(1080f, 680f);
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
            DrawSummary();
            DrawFilters();

            scroll = EditorGUILayout.BeginScrollView(
                scroll,
                false,
                true,
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            DrawSections();
            DrawValidation();

            EditorGUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label(WindowTitle, EditorStyles.boldLabel, GUILayout.Width(320f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reload / 重新载入", EditorStyles.toolbarButton, GUILayout.Width(150f)))
                {
                    Reload();
                }

                if (GUILayout.Button("Validate / 校验", EditorStyles.toolbarButton, GUILayout.Width(130f)))
                {
                    RunValidation();
                }

                if (GUILayout.Button("Export Reports / 导出报告", EditorStyles.toolbarButton, GUILayout.Width(170f)))
                {
                    ExportReports();
                }
            }

            EditorGUILayout.HelpBox(
                "V0.4 developer tuning data panel preview. It reads BuildSandbox PreviewContext data and exposes Chinese display names with English stable keys for config/report/developer tuning only. It does not create player formal UI, mechanism-specific frames, formal scene edits, or hand-layout changes.",
                MessageType.Info);
        }

        private void DrawSummary()
        {
            EnsurePreview();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawReadOnlyRow("Package", preview.packageName);
                DrawReadOnlyRow("Reference Mode", preview.referenceMode);
                DrawReadOnlyRow("Source Context", preview.sourcePreviewContextPackageName);
                DrawReadOnlyRow("Source Build Id", preview.sourcePreviewBuildId);
                DrawReadOnlyRow("Section Count", preview.SectionCount.ToString());
                DrawReadOnlyRow("Field Count", preview.FieldCount.ToString());
                DrawReadOnlyRow("Player Visible Fields", preview.PlayerVisibleFieldCount.ToString());
                DrawReadOnlyRow("Masked Fields", preview.MaskedFieldCount.ToString());
                DrawReadOnlyRow("Formal UI / Scene Write", $"{preview.writesFormalUi} / {preview.writesFormalScene}");
                DrawReadOnlyRow("Mechanic Frame / Hand Layout Change", $"{preview.createsMechanicUiFrame} / {preview.changesHandTunedLayout}");
            }
        }

        private void DrawFilters()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Search / 搜索", GUILayout.Width(90f));
                searchText = EditorGUILayout.TextField(searchText ?? string.Empty);
                showMaskedOnly = EditorGUILayout.ToggleLeft("Masked Only / 只看遮蔽字段", showMaskedOnly, GUILayout.Width(210f));
            }
        }

        private void DrawSections()
        {
            EnsurePreview();
            foreach (BuildTuningDataPanelSection section in preview.sections ?? new List<BuildTuningDataPanelSection>())
            {
                IReadOnlyList<BuildTuningDataPanelFieldRow> rows = FilterRows(section).ToList();
                if (rows.Count == 0 && !MatchesSection(section))
                {
                    continue;
                }

                string title =
                    $"{section.chineseDisplayName} [{section.englishStableKey}]  rows={rows.Count}/{section.rows.Count}";
                if (!DrawFoldout(section.englishStableKey, title))
                {
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    DrawReadOnlyRow("Section English Stable Key", section.englishStableKey);
                    DrawReadOnlyRow("Section Chinese Display Name", section.chineseDisplayName);
                    DrawReadOnlyRow("Data Panel Slot", section.dataPanelSlot);
                    DrawReadOnlyRow("Source ViewModel Key", section.sourceViewModelKey);
                    DrawReadOnlyRow("Developer Only / Player Visible", $"{section.developerOnly} / {section.playerVisible}");
                    DrawReadOnlyRow("Reference Only / Writes Formal UI", $"{section.referenceOnly} / {section.canWriteFormalUi}");
                    EditorGUILayout.Space(4f);

                    foreach (BuildTuningDataPanelFieldRow row in rows)
                    {
                        DrawFieldRow(row);
                    }
                }
            }
        }

        private void DrawFieldRow(BuildTuningDataPanelFieldRow row)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("English Stable Key", GUILayout.Width(150f));
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField(row.englishStableKey ?? string.Empty, GUILayout.MinWidth(260f));
                    }

                    EditorGUILayout.LabelField("中文显示名", GUILayout.Width(90f));
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.TextField(row.chineseDisplayName ?? string.Empty, GUILayout.MinWidth(180f));
                    }
                }

                DrawReadOnlyTextArea("Value", row.value);
                DrawReadOnlyRow("Source Data Path", row.sourceDataPath);
                DrawReadOnlyRow("Developer / Player / Masked", $"{row.developerVisible} / {row.playerVisible} / {row.maskedFromPlayer}");
            }
        }

        private void DrawValidation()
        {
            if (validationReports == null)
            {
                return;
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Validation / 校验", EditorStyles.boldLabel);
            int errors = validationReports.Sum(report => report.ErrorCount);
            int warnings = validationReports.Sum(report => report.WarningCount);
            int infos = validationReports.Sum(report => report.InfoCount);
            EditorGUILayout.LabelField($"Error: {errors}    Warning: {warnings}    Info: {infos}", EditorStyles.boldLabel);
            showValidationInfo = EditorGUILayout.ToggleLeft("Show Info / 显示 Info", showValidationInfo);

            foreach (BuildSandboxValidationReport report in validationReports)
            {
                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField(
                    $"{report.Name} - {(report.Passed ? "PASS" : "FAIL")} / E:{report.ErrorCount} W:{report.WarningCount} I:{report.InfoCount}",
                    EditorStyles.boldLabel);

                foreach (BuildSandboxValidationIssue issue in report.Issues)
                {
                    if (!showValidationInfo && issue.Level == BuildSandboxValidationLevel.Info)
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

        private IEnumerable<BuildTuningDataPanelFieldRow> FilterRows(BuildTuningDataPanelSection section)
        {
            foreach (BuildTuningDataPanelFieldRow row in section?.rows ?? new List<BuildTuningDataPanelFieldRow>())
            {
                if (row == null)
                {
                    continue;
                }

                if (showMaskedOnly && !row.maskedFromPlayer)
                {
                    continue;
                }

                if (Matches(row.englishStableKey, row.chineseDisplayName, row.value, row.sourceDataPath))
                {
                    yield return row;
                }
            }
        }

        private bool MatchesSection(BuildTuningDataPanelSection section)
        {
            return section != null && Matches(section.englishStableKey, section.chineseDisplayName, section.dataPanelSlot, section.sourceViewModelKey);
        }

        private bool Matches(params string[] values)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return true;
            }

            string filter = searchText.Trim();
            return values.Any(value =>
                !string.IsNullOrWhiteSpace(value)
                && value.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
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

        private void RunValidation()
        {
            validationReports = BuildTuningDataPanelPreviewValidator.BuildValidationReports();
        }

        private void ExportReports()
        {
            validationReports = BuildTuningDataPanelPreviewValidator.BuildValidationReports();
            BuildTuningDataPanelPreview currentPreview =
                BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
            string[] paths = BuildTuningDataPanelPreviewReportWriter.WriteReports(validationReports, currentPreview);
            preview = currentPreview;
            Debug.Log($"[BuildTuningDataPanelPreview01] reports exported: {string.Join(", ", paths)}");
        }

        private void Reload()
        {
            preview = BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
            validationReports = null;
            foldouts.Clear();
        }

        private void EnsurePreview()
        {
            preview ??= BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
        }

        private static void DrawReadOnlyRow(string label, string value)
        {
            using (new EditorGUI.DisabledScope(true))
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(220f));
                EditorGUILayout.TextField(value ?? string.Empty, GUILayout.ExpandWidth(true));
            }
        }

        private static void DrawReadOnlyTextArea(string label, string value)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextArea(value ?? string.Empty, GUILayout.MinHeight(34f));
            }
        }
    }
}
#endif
