#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class MechanicHintFeedbackPreviewValidator
    {
        public const string PackageName = MechanicHintFeedbackPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/MechanicHintFeedbackPreview01/[QA Only] Run Mechanic Hint Feedback Preview";

        private static readonly string[] RequiredCategories =
        {
            "mapMechanic",
            "enemyMechanic",
            "bossSkill",
            "failureFeedback"
        };

        private static readonly string[] AllowedReuseChannels =
        {
            "BattleHint",
            "DamageFeedback",
            "CombatLog",
            "Tooltip",
            "BossInfo"
        };

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "Boss",
            "Boss六钥匙",
            "Boss 六钥匙",
            "previewWeight",
            "keyRequirements",
            "BuildProblemSeedDataset"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxPreviewContextValidator.BuildValidationReports();
            reports.Add(BattlePageViewAdapterValidator.Validate());
            reports.Add(BuildTuningDataPanelPreviewValidator.Validate());
            reports.Add(Validate());
            return reports;
        }

        public static MechanicHintFeedbackPreview BuildDefaultPreview()
        {
            BuildSandboxPreviewContext context =
                BuildSandboxPreviewContextValidator.BuildDefaultContext();
            BattlePageViewAdapter adapter =
                BattlePageViewAdapterValidator.BuildDefaultAdapter();
            BuildTuningDataPanelPreview dataPanel =
                BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
            return MechanicHintFeedbackPreviewBuilder.Build(context, adapter.spec, dataPanel);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Mechanic Hint Feedback Preview 01");
            MechanicHintFeedbackPreview preview = BuildDefaultPreview();
            BuildTuningDataPanelPreview dataPanel =
                BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();

            ValidateIsolation(report, preview);
            ValidateRows(report, preview);
            ValidateCategoryCoverage(report, preview);
            ValidateReuse(report, preview);
            ValidatePlayerText(report, preview);
            ValidateDeveloperDataPanelLinks(report, preview, dataPanel);
            ValidateFeedbackExtension(report, preview);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            MechanicHintFeedbackPreview preview)
        {
            if (preview == null)
            {
                report.AddError("MECHANIC_HINT_PREVIEW_NULL", "MechanicHintFeedbackPreview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "MECHANIC_HINT_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(MechanicHintFeedbackPreview));
            }

            if (!preview.devOnly)
            {
                report.AddError(
                    "MECHANIC_HINT_DEVONLY_FALSE",
                    "Mechanic hint preview must remain devOnly=true.",
                    nameof(MechanicHintFeedbackPreview));
            }

            ValidateFalse(report, "MECHANIC_HINT_ENABLED_TRUE", preview.isEnabled);
            ValidateFalse(report, "MECHANIC_HINT_WRITES_FORMAL_UI", preview.writesFormalUi);
            ValidateFalse(report, "MECHANIC_HINT_WRITES_FORMAL_SCENE", preview.writesFormalScene);
            ValidateFalse(report, "MECHANIC_HINT_CHANGES_HAND_LAYOUT", preview.changesHandTunedLayout);
            ValidateFalse(report, "MECHANIC_HINT_CREATES_UI_FRAME", preview.createsMechanicUiFrame);
            ValidateFalse(report, "MECHANIC_HINT_CREATES_DEDICATED_PANELS", preview.createsDedicatedMechanicPanels);
            ValidateFalse(report, "MECHANIC_HINT_TOUCHES_RUNFLOW", preview.touchesRunFlow);
            ValidateFalse(report, "MECHANIC_HINT_TOUCHES_PAGESTATE", preview.touchesPageState);
            ValidateFalse(report, "MECHANIC_HINT_TOUCHES_FORMATIONSTATE", preview.touchesFormationState);
            ValidateFalse(report, "MECHANIC_HINT_TOUCHES_SAVE", preview.touchesSaveData);
            ValidateFalse(report, "MECHANIC_HINT_TOUCHES_BOSS_REWARD_NUMERIC", preview.touchesBossRewardDropNumeric);
            ValidateFalse(report, "MECHANIC_HINT_PLAYER_SHOWS_ENGLISH_KEYS", preview.playerUiShowsEnglishStableKey);
            ValidateFalse(report, "MECHANIC_HINT_PLAYER_SHOWS_FULL_ANSWERS", preview.playerUiShowsFullAnswers);

            if (!preview.playerUiChineseOnly)
            {
                report.AddError(
                    "MECHANIC_HINT_PLAYER_CHINESE_ONLY_FALSE",
                    "Player-side hint preview must be Chinese-only.",
                    nameof(MechanicHintFeedbackPreview));
            }

            if (!preview.developerFullAnswersStayInDataPanel)
            {
                report.AddError(
                    "MECHANIC_HINT_DEV_PANEL_RULE_FALSE",
                    "Full solution fields must stay in the developer data panel.",
                    nameof(MechanicHintFeedbackPreview));
            }

            if (!string.Equals(preview.referenceMode, MechanicHintFeedbackPreview.ReferenceMode, StringComparison.Ordinal))
            {
                report.AddError(
                    "MECHANIC_HINT_REFERENCE_MODE_INVALID",
                    $"Reference mode mismatch. actual={preview.referenceMode}.",
                    nameof(MechanicHintFeedbackPreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "MECHANIC_HINT_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "MECHANIC_HINT_ISOLATION_PASS",
                    "Preview is devOnly, disabled, data-only, and disconnected from formal flow/UI/scene/save surfaces.",
                    nameof(MechanicHintFeedbackPreview));
            }
        }

        private static void ValidateRows(
            BuildSandboxValidationReport report,
            MechanicHintFeedbackPreview preview)
        {
            IReadOnlyList<MechanicHintFeedbackPreviewRow> rows = Rows(preview);
            ValidateMinimum(report, "MECHANIC_HINT_ROW_COUNT", "Player hint row", rows.Count, 30);

            HashSet<string> keys = new(StringComparer.Ordinal);
            foreach (MechanicHintFeedbackPreviewRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("MECHANIC_HINT_ROW_NULL", "Null hint row.", nameof(MechanicHintFeedbackPreviewRow));
                    continue;
                }

                ValidateLabelPair(report, row.englishStableKey, row.chineseDisplayName, nameof(MechanicHintFeedbackPreviewRow));
                if (!keys.Add(row.englishStableKey))
                {
                    report.AddError(
                        "MECHANIC_HINT_ROW_DUPLICATE_KEY",
                        $"Duplicate English stable key: {row.englishStableKey}.",
                        nameof(MechanicHintFeedbackPreviewRow));
                }

                if (string.IsNullOrWhiteSpace(row.category)
                    || string.IsNullOrWhiteSpace(row.sourceDataPath)
                    || string.IsNullOrWhiteSpace(row.developerDataPanelFieldKey))
                {
                    report.AddError(
                        "MECHANIC_HINT_ROW_SOURCE_MISSING",
                        $"Hint row needs category, source path, and developer data panel key. key={row.englishStableKey}.",
                        nameof(MechanicHintFeedbackPreviewRow));
                }

                if (!row.playerVisible
                    || row.maskedFromPlayer
                    || !row.developerPanelVisible
                    || row.canWriteFormalUi
                    || row.createsDedicatedPanel
                    || row.exposesFullAnswerToPlayer)
                {
                    report.AddError(
                        "MECHANIC_HINT_ROW_SCOPE_LEAK",
                        $"Player hint row must be visible as masked copy only through existing language, without formal writes or answer exposure. key={row.englishStableKey}.",
                        nameof(MechanicHintFeedbackPreviewRow));
                }
            }
        }

        private static void ValidateCategoryCoverage(
            BuildSandboxValidationReport report,
            MechanicHintFeedbackPreview preview)
        {
            IReadOnlyList<MechanicHintFeedbackPreviewRow> rows = Rows(preview);
            foreach (string category in RequiredCategories)
            {
                int count = rows.Count(row => row != null && string.Equals(row.category, category, StringComparison.Ordinal));
                ValidateMinimum(report, $"MECHANIC_HINT_CATEGORY_{category.ToUpperInvariant()}", category, count, 1);
            }

            ValidateMinimum(
                report,
                "MECHANIC_HINT_CATEGORY_DROP_BIAS_ATMOSPHERE",
                "drop bias atmosphere row",
                rows.Count(row => row != null && row.category == "dropBiasAtmosphere"),
                1);
            ValidateMinimum(
                report,
                "MECHANIC_HINT_CATEGORY_WEAKNESS_WINDOW",
                "weakness window row",
                rows.Count(row => row != null && row.category == "weaknessWindow"),
                1);
        }

        private static void ValidateReuse(
            BuildSandboxValidationReport report,
            MechanicHintFeedbackPreview preview)
        {
            IReadOnlyList<MechanicHintFeedbackPreviewRow> rows = Rows(preview);
            foreach (MechanicHintFeedbackPreviewRow row in rows)
            {
                if (row == null)
                {
                    continue;
                }

                if (!AllowedReuseChannels.Contains(row.reuseChannel, StringComparer.Ordinal))
                {
                    report.AddError(
                        "MECHANIC_HINT_REUSE_CHANNEL_INVALID",
                        $"Hint row must reuse a mature feedback channel. key={row.englishStableKey}, channel={row.reuseChannel}.",
                        nameof(MechanicHintFeedbackPreviewRow));
                }

                if (string.IsNullOrWhiteSpace(row.reuseSurface))
                {
                    report.AddError(
                        "MECHANIC_HINT_REUSE_SURFACE_MISSING",
                        $"Hint row needs reuse surface notes. key={row.englishStableKey}.",
                        nameof(MechanicHintFeedbackPreviewRow));
                }
            }

            ValidateMinimum(report, "MECHANIC_HINT_REUSE_CHANNEL_COUNT", "Reuse channel", preview?.ReuseChannelCount ?? 0, 4);
        }

        private static void ValidatePlayerText(
            BuildSandboxValidationReport report,
            MechanicHintFeedbackPreview preview)
        {
            foreach (MechanicHintFeedbackPreviewRow row in preview?.playerRows ?? new List<MechanicHintFeedbackPreviewRow>())
            {
                if (row == null)
                {
                    continue;
                }

                ValidatePlayerChinese(report, row.playerChineseText, row.englishStableKey);
                ValidatePlayerChinese(report, row.atmosphereChinese, row.englishStableKey);
            }
        }

        private static void ValidateDeveloperDataPanelLinks(
            BuildSandboxValidationReport report,
            MechanicHintFeedbackPreview preview,
            BuildTuningDataPanelPreview dataPanel)
        {
            IReadOnlyList<MechanicHintDeveloperDataPanelLink> links = DeveloperLinks(preview);
            HashSet<string> linkKeys = new(links.Select(link => link?.englishStableKey ?? string.Empty), StringComparer.Ordinal);
            IEnumerable<string> fieldKeys = dataPanel != null && dataPanel.Rows != null
                ? dataPanel.Rows.Select(row => row.englishStableKey)
                : Array.Empty<string>();
            HashSet<string> dataPanelKeys = new(fieldKeys, StringComparer.Ordinal);

            foreach (string key in MechanicHintFeedbackPreviewBuilder.SensitiveKeys)
            {
                if (!linkKeys.Contains(key))
                {
                    report.AddError(
                        "MECHANIC_HINT_DEV_PANEL_LINK_MISSING",
                        $"Missing developer data panel link: {key}.",
                        nameof(MechanicHintDeveloperDataPanelLink));
                }

                if (!dataPanelKeys.Contains(key))
                {
                    report.AddError(
                        "MECHANIC_HINT_DATA_PANEL_FIELD_MISSING",
                        $"Developer data panel is missing sensitive field: {key}.",
                        nameof(BuildTuningDataPanelFieldRow));
                }
            }

            foreach (MechanicHintDeveloperDataPanelLink link in links)
            {
                if (link == null)
                {
                    report.AddError("MECHANIC_HINT_DEV_LINK_NULL", "Null developer data panel link.", nameof(MechanicHintDeveloperDataPanelLink));
                    continue;
                }

                ValidateLabelPair(report, link.englishStableKey, link.chineseDisplayName, nameof(MechanicHintDeveloperDataPanelLink));
                if (!link.developerVisible || link.playerVisible || !link.maskedFromPlayer)
                {
                    report.AddError(
                        "MECHANIC_HINT_DEV_LINK_SCOPE_LEAK",
                        $"Developer link must be masked from player UI. key={link.englishStableKey}.",
                        nameof(MechanicHintDeveloperDataPanelLink));
                }
            }
        }

        private static void ValidateFeedbackExtension(
            BuildSandboxValidationReport report,
            MechanicHintFeedbackPreview preview)
        {
            BattleFeedbackMechanicHintExtensionViewModel extension = preview?.feedbackExtension;
            if (extension == null)
            {
                report.AddError("MECHANIC_HINT_FEEDBACK_EXTENSION_NULL", "BattleFeedbackMechanicHintExtension output is missing.", PackageName);
                return;
            }

            if (!string.Equals(extension.extensionId, "BattleFeedbackMechanicHintExtension", StringComparison.Ordinal)
                || extension.canWriteFormalUi
                || !extension.requiresExplicitRuntimeBinding)
            {
                report.AddError(
                    "MECHANIC_HINT_FEEDBACK_EXTENSION_SCOPE_LEAK",
                    "Feedback extension must remain a non-formal adapter requiring explicit future binding.",
                    nameof(BattleFeedbackMechanicHintExtensionViewModel));
            }

            ValidateMinimum(
                report,
                "MECHANIC_HINT_FEEDBACK_EXTENSION_ROW_COUNT",
                "Feedback extension row",
                extension.rows?.Count ?? 0,
                30);
        }

        private static void ValidatePlayerChinese(
            BuildSandboxValidationReport report,
            string value,
            string key)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                report.AddError(
                    "MECHANIC_HINT_PLAYER_TEXT_EMPTY",
                    $"Player hint text is empty. key={key}.",
                    nameof(MechanicHintFeedbackPreviewRow));
                return;
            }

            if (!value.Any(character => character > 127))
            {
                report.AddError(
                    "MECHANIC_HINT_PLAYER_TEXT_NOT_CHINESE",
                    $"Player hint text must contain Chinese/non-ASCII display text. key={key}.",
                    nameof(MechanicHintFeedbackPreviewRow));
            }

            if (value.Any(character => character <= 127 && char.IsLetter(character)))
            {
                report.AddError(
                    "MECHANIC_HINT_PLAYER_TEXT_HAS_LATIN",
                    $"Player hint text contains Latin letters. key={key}, text={value}.",
                    nameof(MechanicHintFeedbackPreviewRow));
            }

            foreach (string token in ForbiddenPlayerAnswerTokens)
            {
                if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    report.AddError(
                        "MECHANIC_HINT_PLAYER_TEXT_FORBIDDEN_TOKEN",
                        $"Player hint text exposes forbidden answer token: {token}. key={key}.",
                        nameof(MechanicHintFeedbackPreviewRow));
                }
            }
        }

        private static IReadOnlyList<MechanicHintFeedbackPreviewRow> Rows(
            MechanicHintFeedbackPreview preview)
        {
            return preview != null && preview.playerRows != null
                ? preview.playerRows
                : Array.Empty<MechanicHintFeedbackPreviewRow>();
        }

        private static IReadOnlyList<MechanicHintDeveloperDataPanelLink> DeveloperLinks(
            MechanicHintFeedbackPreview preview)
        {
            return preview != null && preview.developerDataPanelLinks != null
                ? preview.developerDataPanelLinks
                : Array.Empty<MechanicHintDeveloperDataPanelLink>();
        }

        private static void ValidateLabelPair(
            BuildSandboxValidationReport report,
            string englishStableKey,
            string chineseDisplayName,
            string path)
        {
            if (string.IsNullOrWhiteSpace(englishStableKey)
                || string.IsNullOrWhiteSpace(chineseDisplayName))
            {
                report.AddError("MECHANIC_HINT_LABEL_PAIR_MISSING", "Chinese display name and English stable key are required.", path);
                return;
            }

            if (!englishStableKey.All(character =>
                    character <= 127
                    && (char.IsLetterOrDigit(character)
                        || character == '_'
                        || character == '-'
                        || character == '.')))
            {
                report.AddError(
                    "MECHANIC_HINT_ENGLISH_KEY_INVALID",
                    $"English stable key must be ASCII and stable. key={englishStableKey}.",
                    path);
            }

            if (!chineseDisplayName.Any(character => character > 127))
            {
                report.AddError(
                    "MECHANIC_HINT_CHINESE_NAME_MISSING",
                    $"Chinese display name must contain Chinese text. key={englishStableKey}.",
                    path);
            }
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "Expected false for this MechanicHint isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
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
