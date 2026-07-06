#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleFeedbackReadabilityValidator
    {
        public const string PackageName = BattleFeedbackReadabilityPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleFeedbackReadability01/[QA Only] Run Battle Feedback Readability";

        [MenuItem(QaMenuPath)]
        public static void RunFromMenu()
        {
            BattleFeedbackReadabilityPreview preview = BuildDefaultPreview();
            List<BuildSandboxValidationReport> reports = BuildValidationReports(preview);
            string[] paths = BattleFeedbackReadabilityReportWriter.WriteReports(reports, preview);
            Debug.Log($"Battle feedback readability reports written: {string.Join(", ", paths)}");
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports(
            BattleFeedbackReadabilityPreview preview = null)
        {
            BattleFeedbackReadabilityPreview safePreview = preview ?? BuildDefaultPreview();
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                Validate(safePreview)
            };
        }

        public static BattleFeedbackReadabilityPreview BuildDefaultPreview()
        {
            return BattleFeedbackReadabilityBuilder.BuildDefaultPreview();
        }

        public static BuildSandboxValidationReport Validate(
            BattleFeedbackReadabilityPreview preview = null)
        {
            BattleFeedbackReadabilityPreview safePreview = preview ?? BuildDefaultPreview();
            BuildSandboxValidationReport report = new("Battle Feedback Readability 01");

            ValidateIsolation(report, safePreview);
            ValidateCoverage(report, safePreview);
            ValidateRows(report, safePreview);
            ValidateLeakCounters(report, safePreview);
            return report;
        }

        public static int CountPlayerTextLeaks(BattleFeedbackReadabilityPreview preview)
        {
            return BattleFeedbackReadabilityBuilder.CountPlayerTextLeaks(preview);
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleFeedbackReadabilityPreview preview)
        {
            if (preview == null)
            {
                report.AddError("BATTLE_FEEDBACK_READABILITY_NULL", "Readability preview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "BATTLE_FEEDBACK_READABILITY_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(BattleFeedbackReadabilityPreview));
            }

            ValidateTrue(report, "BATTLE_FEEDBACK_DEVONLY_TRUE", preview.devOnly);
            ValidateFalse(report, "BATTLE_FEEDBACK_ENABLED_FALSE", preview.isEnabled);
            ValidateTrue(report, "BATTLE_FEEDBACK_READS_BOARD", preview.readsCurrentBoardSnapshot);
            ValidateTrue(report, "BATTLE_FEEDBACK_READS_BUILD_PREVIEW", preview.readsBuildCombatPreview);
            ValidateTrue(report, "BATTLE_FEEDBACK_READS_RUNTIME_LOOP", preview.readsRuntimeLoopPreview);
            ValidateTrue(report, "BATTLE_FEEDBACK_READS_ENERGY_CONTRACT", preview.readsFormationEnergyContract);
            ValidateTrue(report, "BATTLE_FEEDBACK_READS_LEGACY_BEHAVIOR", preview.readsLegacyItemBehavior);
            ValidateTrue(report, "BATTLE_FEEDBACK_WRITES_EXISTING_TEXT", preview.writesExistingFeedbackText);
            ValidateFalse(report, "BATTLE_FEEDBACK_WRITES_FORMAL_FLOW", preview.writesFormalFlow);
            ValidateFalse(report, "BATTLE_FEEDBACK_WRITES_SAVE", preview.writesFormalSaveData);
            ValidateFalse(report, "BATTLE_FEEDBACK_WRITES_REWARD", preview.writesFormalReward);
            ValidateFalse(report, "BATTLE_FEEDBACK_ADVANCES_CHAPTER", preview.advancesChapter);
            ValidateFalse(report, "BATTLE_FEEDBACK_OPENS_FEATURE_FLAG", preview.opensFeatureFlag);
            ValidateFalse(report, "BATTLE_FEEDBACK_WRITES_SCENE_LAYOUT", preview.writesSceneLayout);
            ValidateFalse(report, "BATTLE_FEEDBACK_SHOWS_FULL_ANSWERS", preview.playerUiShowsFullAnswers);

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "BATTLE_FEEDBACK_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if (!preview.DevOnlyIsolationPass)
            {
                report.AddError(
                    "BATTLE_FEEDBACK_ISOLATION_FAIL",
                    "Readability preview must stay devOnly, disabled, no formal flow/save/reward/chapter/flag/layout writes.",
                    nameof(BattleFeedbackReadabilityPreview));
            }
        }

        private static void ValidateCoverage(
            BuildSandboxValidationReport report,
            BattleFeedbackReadabilityPreview preview)
        {
            ValidateMinimum(report, "BATTLE_FEEDBACK_ROW_COUNT", "Readability row", preview?.RowCount ?? 0, 20);
            ValidateMinimum(report, "BATTLE_FEEDBACK_CHANNEL_MAP_COUNT", "Channel map row", preview?.ChannelMapRowCount ?? 0, 20);
            ValidateMinimum(report, "BATTLE_FEEDBACK_SOURCE_BUILD_ROWS", "Source build combat preview row", preview?.sourceBuildCombatPreviewRowCount ?? 0, 1);
            ValidateMinimum(report, "BATTLE_FEEDBACK_SOURCE_RUNTIME_ROWS", "Source runtime loop row", preview?.sourceRuntimeLoopRowCount ?? 0, 1);

            foreach (string category in RequiredCategories())
            {
                int count = CountCategory(preview, category);
                ValidateMinimum(report, $"BATTLE_FEEDBACK_CATEGORY_{category}", $"{category} sample", count, 1);
            }

            ValidateTrue(report, "BATTLE_FEEDBACK_ENERGY_NONE", preview?.CoversEnergyNone ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_ENERGY_WEAK_PULSE", preview?.CoversEnergyWeakPulse ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_ENERGY_POWERED", preview?.CoversEnergyPowered ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_ENERGY_SUPPRESSED", preview?.CoversEnergySuppressed ?? false);
            ValidateMinimum(report, "BATTLE_FEEDBACK_ENERGY_STATE_COUNT", "EnergyState coverage", preview?.EnergyStateCoverageCount ?? 0, 4);
            ValidateMinimum(report, "BATTLE_FEEDBACK_LEGACY_ITEM_TRIGGERS", "Legacy item trigger feedback", preview?.LegacyItemTriggerFeedbackCount ?? 0, 5);
            ValidateTrue(report, "BATTLE_FEEDBACK_DAMAGE_SAMPLE", preview?.HasDamageFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_SHIELD_SAMPLE", preview?.HasShieldFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_HEAL_SAMPLE", preview?.HasHealFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_CLEANSE_SAMPLE", preview?.HasCleanseFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_CONTROL_SAMPLE", preview?.HasControlFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_BOSS_CAST_SAMPLE", preview?.HasBossCastFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_FLOATING_SAMPLE", preview?.HasMechanicFloatingFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_BUILD_FUZZY", preview?.HasBuildFuzzyFeedback ?? false);
            ValidateTrue(report, "BATTLE_FEEDBACK_FAILURE_FUZZY", preview?.HasFailureFuzzyHint ?? false);
        }

        private static void ValidateRows(
            BuildSandboxValidationReport report,
            BattleFeedbackReadabilityPreview preview)
        {
            HashSet<string> ids = new(StringComparer.Ordinal);
            foreach (BattleFeedbackReadabilityRow row in Rows(preview))
            {
                if (row == null)
                {
                    report.AddError("BATTLE_FEEDBACK_ROW_NULL", "Null readability row.", nameof(BattleFeedbackReadabilityRow));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.feedbackKey)
                    || string.IsNullOrWhiteSpace(row.category)
                    || string.IsNullOrWhiteSpace(row.feedbackKind)
                    || string.IsNullOrWhiteSpace(row.reuseChannel)
                    || string.IsNullOrWhiteSpace(row.sourceDataPath)
                    || string.IsNullOrWhiteSpace(row.developerDataPanelFieldKey))
                {
                    report.AddError(
                        "BATTLE_FEEDBACK_ROW_IDENTITY_MISSING",
                        $"Readability row needs key, category, kind, channel, source, and developer field. key={row.feedbackKey}.",
                        nameof(BattleFeedbackReadabilityRow));
                }

                if (!ids.Add(row.feedbackKey ?? string.Empty))
                {
                    report.AddError(
                        "BATTLE_FEEDBACK_DUPLICATE_KEY",
                        $"Duplicate readability key: {row.feedbackKey}.",
                        nameof(BattleFeedbackReadabilityRow));
                }

                foreach (string value in BattleFeedbackReadabilityBuilder.PlayerTextFields(row))
                {
                    ValidatePlayerText(report, value, row.feedbackKey);
                }

                if (row.PlayerTextLeak)
                {
                    report.AddError(
                        "BATTLE_FEEDBACK_PLAYER_TEXT_LEAK",
                        $"Player-facing text has Latin/stable key/answer token leak. key={row.feedbackKey}.",
                        nameof(BattleFeedbackReadabilityRow));
                }

                if (row.FormalLeak)
                {
                    report.AddError(
                        "BATTLE_FEEDBACK_ROW_FORMAL_LEAK",
                        $"Readability row must not connect formal UI/flow/save/reward/chapter/flag/layout writes. key={row.feedbackKey}.",
                        nameof(BattleFeedbackReadabilityRow));
                }
            }
        }

        private static void ValidateLeakCounters(
            BuildSandboxValidationReport report,
            BattleFeedbackReadabilityPreview preview)
        {
            if ((preview?.FeatureFlagDefaultTrueCount ?? 1) != 0)
            {
                report.AddError(
                    "BATTLE_FEEDBACK_FEATURE_FLAG_DEFAULT_TRUE",
                    $"Feature flag default true count must be 0. actual={preview?.FeatureFlagDefaultTrueCount ?? 1}.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if ((preview?.PlayerTextLeakCount ?? 1) != 0)
            {
                report.AddError(
                    "BATTLE_FEEDBACK_PLAYER_LEAK_COUNT_NONZERO",
                    $"Player text leak count must be 0. actual={preview?.PlayerTextLeakCount ?? 1}.",
                    nameof(BattleFeedbackReadabilityPreview));
            }

            if ((preview?.FormalFlowLeakCount ?? 1) != 0)
            {
                report.AddError(
                    "BATTLE_FEEDBACK_FORMAL_LEAK_COUNT_NONZERO",
                    $"Formal leak count must be 0. actual={preview?.FormalFlowLeakCount ?? 1}.",
                    nameof(BattleFeedbackReadabilityPreview));
            }

            if ((preview?.TotalLeakCount ?? 1) != 0)
            {
                report.AddError(
                    "BATTLE_FEEDBACK_TOTAL_LEAK_COUNT_NONZERO",
                    $"Leak Check must be 0. actual={preview?.TotalLeakCount ?? 1}.",
                    nameof(BattleFeedbackReadabilityPreview));
            }
        }

        private static void ValidatePlayerText(
            BuildSandboxValidationReport report,
            string value,
            string key)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                report.AddError(
                    "BATTLE_FEEDBACK_PLAYER_TEXT_EMPTY",
                    $"Player text is empty. key={key}.",
                    nameof(BattleFeedbackReadabilityRow));
                return;
            }

            if (!BattleFeedbackReadabilityBuilder.ContainsNonAscii(value))
            {
                report.AddError(
                    "BATTLE_FEEDBACK_PLAYER_TEXT_NOT_CHINESE",
                    $"Player text must contain Chinese/non-ASCII display text. key={key}.",
                    nameof(BattleFeedbackReadabilityRow));
            }

            if (BattleFeedbackReadabilityBuilder.ContainsLatin(value))
            {
                report.AddError(
                    "BATTLE_FEEDBACK_PLAYER_TEXT_HAS_LATIN",
                    $"Player text contains Latin letters. key={key}, text={value}.",
                    nameof(BattleFeedbackReadabilityRow));
            }
        }

        private static IEnumerable<string> RequiredCategories()
        {
            yield return BattleFeedbackReadabilityCategories.EnergyFeedback;
            yield return BattleFeedbackReadabilityCategories.ItemTriggerFeedback;
            yield return BattleFeedbackReadabilityCategories.BossFeedback;
            yield return BattleFeedbackReadabilityCategories.BuildFeedback;
            yield return BattleFeedbackReadabilityCategories.FailureHint;
        }

        private static int CountCategory(
            BattleFeedbackReadabilityPreview preview,
            string category)
        {
            return Rows(preview).Count(row => string.Equals(row.category, category, StringComparison.Ordinal));
        }

        private static IReadOnlyList<BattleFeedbackReadabilityRow> Rows(
            BattleFeedbackReadabilityPreview preview)
        {
            return preview != null && preview.rows != null
                ? preview.rows
                : Array.Empty<BattleFeedbackReadabilityRow>();
        }

        private static void ValidateTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (!value)
            {
                report.AddError(code, "Expected true for this battle feedback readability flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Required flag remains true.", PackageName);
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "Expected false for this battle feedback readability isolation flag.", PackageName);
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
