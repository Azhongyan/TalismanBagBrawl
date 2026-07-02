#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildTuningDataPanelPreviewValidator
    {
        public const string PackageName = BuildTuningDataPanelPreview.PackageName;

        private static readonly string[] RequiredSectionKeys =
        {
            "buildSummary",
            "synergySummary",
            "shapeOccupancy",
            "affixModifier",
            "problemReadiness",
            "simulationResult",
            "dropBiasPreview",
            "bossSixKeyFullAnswer"
        };

        private static readonly string[] RequiredSensitiveFieldKeys =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "dropBiasWeights",
            "bossSixKeyFullAnswer"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxPreviewContextValidator.BuildValidationReports();
            reports.Add(BattlePageViewAdapterValidator.Validate());
            reports.Add(Validate());
            return reports;
        }

        public static BuildTuningDataPanelPreview BuildDefaultPreview()
        {
            BuildSandboxPreviewContext context =
                BuildSandboxPreviewContextValidator.BuildDefaultContext();
            BattlePageViewAdapter adapter =
                BattlePageViewAdapterValidator.BuildDefaultAdapter();
            return BuildTuningDataPanelPreviewBuilder.Build(context, adapter.spec);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Build Tuning Data Panel Preview 01");
            BuildTuningDataPanelPreview preview = BuildDefaultPreview();
            ValidateIsolation(report, preview);
            ValidateSections(report, preview);
            ValidateFields(report, preview);
            ValidateSensitiveFields(report, preview);
            ValidateCoverage(report, preview);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BuildTuningDataPanelPreview preview)
        {
            if (preview == null)
            {
                report.AddError("BUILD_TUNING_PANEL_NULL", "BuildTuningDataPanelPreview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "BUILD_TUNING_PANEL_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(BuildTuningDataPanelPreview));
            }

            if (!preview.devOnly)
            {
                report.AddError(
                    "BUILD_TUNING_PANEL_DEVONLY_FALSE",
                    "Developer tuning data panel must remain devOnly=true.",
                    nameof(BuildTuningDataPanelPreview));
            }

            ValidateFalse(report, "BUILD_TUNING_PANEL_ENABLED_TRUE", preview.isEnabled, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_PLAYER_FORMAL_UI", preview.playerFormalUi, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_WRITES_FORMAL_UI", preview.writesFormalUi, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_WRITES_FORMAL_SCENE", preview.writesFormalScene, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_READS_FORMAL_SAVE", preview.readsFormalSaveData, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_WRITES_FORMAL_DATA", preview.writesFormalData, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_CREATES_MECHANIC_FRAME", preview.createsMechanicUiFrame, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_CHANGES_HAND_LAYOUT", preview.changesHandTunedLayout, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_PLAYER_SHOWS_ENGLISH_KEYS", preview.playerUiShowsEnglishStableKey, nameof(BuildTuningDataPanelPreview));
            ValidateFalse(report, "BUILD_TUNING_PANEL_PLAYER_SHOWS_FULL_ANSWERS", preview.playerUiShowsFullAnswers, nameof(BuildTuningDataPanelPreview));

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "BUILD_TUNING_PANEL_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "BUILD_TUNING_PANEL_ISOLATION_PASS",
                    "Panel is developer-only, disabled, report-only, and not connected to formal UI/scene/save/data.",
                    nameof(BuildTuningDataPanelPreview));
            }
        }

        private static void ValidateSections(
            BuildSandboxValidationReport report,
            BuildTuningDataPanelPreview preview)
        {
            if (preview?.sections == null)
            {
                report.AddError("BUILD_TUNING_PANEL_SECTIONS_NULL", "Panel sections are missing.", PackageName);
                return;
            }

            ValidateMinimum(
                report,
                "BUILD_TUNING_PANEL_SECTION_COUNT",
                "Panel section",
                preview.SectionCount,
                RequiredSectionKeys.Length);

            HashSet<string> actualKeys = new(
                preview.sections.Select(section => section?.englishStableKey ?? string.Empty),
                StringComparer.Ordinal);
            foreach (string required in RequiredSectionKeys)
            {
                if (!actualKeys.Contains(required))
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_SECTION_MISSING",
                        $"Missing developer panel section: {required}.",
                        nameof(BuildTuningDataPanelSection));
                }
            }

            foreach (BuildTuningDataPanelSection section in preview.sections)
            {
                if (section == null)
                {
                    report.AddError("BUILD_TUNING_PANEL_SECTION_NULL", "Null panel section.", nameof(BuildTuningDataPanelSection));
                    continue;
                }

                ValidateLabelPair(report, "BUILD_TUNING_PANEL_SECTION_LABEL", section.englishStableKey, section.chineseDisplayName, nameof(BuildTuningDataPanelSection));
                if (string.IsNullOrWhiteSpace(section.dataPanelSlot))
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_SLOT_MISSING",
                        $"Section needs a V04 data panel slot. key={section.englishStableKey}.",
                        nameof(BuildTuningDataPanelSection));
                }

                if (!section.developerOnly
                    || section.playerVisible
                    || !section.referenceOnly
                    || section.canWriteFormalUi
                    || section.createsMechanicUiFrame)
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_SECTION_SCOPE_LEAK",
                        $"Section must remain developer-only, reference-only, and non-formal. key={section.englishStableKey}.",
                        nameof(BuildTuningDataPanelSection));
                }

                if ((section.rows?.Count ?? 0) == 0)
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_SECTION_EMPTY",
                        $"Section has no field rows. key={section.englishStableKey}.",
                        nameof(BuildTuningDataPanelSection));
                }
            }
        }

        private static void ValidateFields(
            BuildSandboxValidationReport report,
            BuildTuningDataPanelPreview preview)
        {
            IReadOnlyList<BuildTuningDataPanelFieldRow> rows =
                preview?.Rows ?? Array.Empty<BuildTuningDataPanelFieldRow>();
            ValidateMinimum(report, "BUILD_TUNING_PANEL_FIELD_COUNT", "Panel field", rows.Count, 50);

            HashSet<string> keys = new(StringComparer.Ordinal);
            foreach (BuildTuningDataPanelFieldRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("BUILD_TUNING_PANEL_FIELD_NULL", "Null field row.", nameof(BuildTuningDataPanelFieldRow));
                    continue;
                }

                ValidateLabelPair(report, "BUILD_TUNING_PANEL_FIELD_LABEL", row.englishStableKey, row.chineseDisplayName, nameof(BuildTuningDataPanelFieldRow));
                if (!keys.Add(row.englishStableKey))
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_FIELD_DUPLICATE_KEY",
                        $"Duplicate English stable key: {row.englishStableKey}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }

                if (string.IsNullOrWhiteSpace(row.sourceDataPath))
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_FIELD_SOURCE_MISSING",
                        $"Field needs a source data path. key={row.englishStableKey}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }

                if (string.IsNullOrWhiteSpace(row.dataPanelSlot))
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_FIELD_SLOT_MISSING",
                        $"Field needs a V04 data panel slot. key={row.englishStableKey}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }

                if (!row.developerVisible || row.playerVisible || !row.reportOnly || !row.devOnly)
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_FIELD_SCOPE_LEAK",
                        $"Field must be developer-visible only, report-only, and devOnly. key={row.englishStableKey}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }
            }

            if (preview != null && preview.PlayerVisibleFieldCount == 0)
            {
                report.AddInfo(
                    "BUILD_TUNING_PANEL_NO_PLAYER_FIELDS",
                    "No field row is visible to player UI.",
                    nameof(BuildTuningDataPanelPreview));
            }
        }

        private static void ValidateSensitiveFields(
            BuildSandboxValidationReport report,
            BuildTuningDataPanelPreview preview)
        {
            IReadOnlyList<BuildTuningDataPanelFieldRow> rows =
                preview?.Rows ?? Array.Empty<BuildTuningDataPanelFieldRow>();
            HashSet<string> actual = new(rows.Select(row => row.englishStableKey), StringComparer.Ordinal);
            foreach (string required in RequiredSensitiveFieldKeys)
            {
                if (!actual.Contains(required))
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_SENSITIVE_FIELD_MISSING",
                        $"Missing required masked developer field: {required}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }
            }

            foreach (BuildTuningDataPanelFieldRow row in rows.Where(row => row != null && IsSensitiveKey(row.englishStableKey)))
            {
                if (!row.maskedFromPlayer || row.playerVisible || !row.developerVisible)
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_SENSITIVE_FIELD_LEAK",
                        $"Sensitive field must be masked from player UI and visible only to developer panel. key={row.englishStableKey}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }
            }

            report.AddInfo(
                "BUILD_TUNING_PANEL_MASKED_FIELD_COUNT",
                $"Masked developer fields={preview?.MaskedFieldCount ?? 0}.",
                nameof(BuildTuningDataPanelPreview));
        }

        private static void ValidateCoverage(
            BuildSandboxValidationReport report,
            BuildTuningDataPanelPreview preview)
        {
            IReadOnlyList<BuildTuningDataPanelFieldRow> rows =
                preview?.Rows ?? Array.Empty<BuildTuningDataPanelFieldRow>();
            string[] requiredPrefixes =
            {
                "buildSummary.",
                "synergySummary.",
                "shapeOccupancy.",
                "affixModifier.",
                "problemReadiness.",
                "simulationResult.",
                "dropBiasPreview.",
                "bossSixKeyFullAnswer."
            };

            foreach (string prefix in requiredPrefixes)
            {
                if (!rows.Any(row => row != null && row.englishStableKey.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    report.AddError(
                        "BUILD_TUNING_PANEL_PREFIX_MISSING",
                        $"No field row found for prefix: {prefix}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }
            }
        }

        private static void ValidateLabelPair(
            BuildSandboxValidationReport report,
            string code,
            string englishStableKey,
            string chineseDisplayName,
            string path)
        {
            if (string.IsNullOrWhiteSpace(englishStableKey)
                || string.IsNullOrWhiteSpace(chineseDisplayName))
            {
                report.AddError(
                    code,
                    "Each field/section needs Chinese display name and English stable key.",
                    path);
                return;
            }

            if (!IsEnglishStableKey(englishStableKey))
            {
                report.AddError(
                    "BUILD_TUNING_PANEL_ENGLISH_KEY_INVALID",
                    $"English stable key must be ASCII and stable. key={englishStableKey}.",
                    path);
            }

            if (!ContainsNonAscii(chineseDisplayName))
            {
                report.AddError(
                    "BUILD_TUNING_PANEL_CHINESE_NAME_MISSING",
                    $"Chinese display name must contain Chinese/non-ASCII display text. key={englishStableKey}.",
                    path);
            }
        }

        private static bool IsSensitiveKey(string englishStableKey)
        {
            return RequiredSensitiveFieldKeys.Any(required =>
                string.Equals(englishStableKey, required, StringComparison.Ordinal)
                || (!string.IsNullOrWhiteSpace(englishStableKey)
                    && englishStableKey.StartsWith(required + ".", StringComparison.Ordinal)));
        }

        private static bool IsEnglishStableKey(string key)
        {
            return !string.IsNullOrWhiteSpace(key)
                && key.All(character =>
                    character <= 127
                    && (char.IsLetterOrDigit(character)
                        || character == '_'
                        || character == '-'
                        || character == '.'));
        }

        private static bool ContainsNonAscii(string value)
        {
            return !string.IsNullOrWhiteSpace(value) && value.Any(character => character > 127);
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string path)
        {
            if (value)
            {
                report.AddError(code, "This isolation flag must stay false.", path);
            }
        }

        private static void ValidateMinimum(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual < expected)
            {
                report.AddError(code, $"{label} count too low. actual={actual}, expected>={expected}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} count pass. actual={actual}, expected>={expected}.", PackageName);
        }
    }
}
#endif
