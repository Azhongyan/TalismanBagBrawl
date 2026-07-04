#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxItemStatCombatPreviewValidator
    {
        public const string PackageName = "V0.4-BattleSandboxItemStatCombatPreview01";
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxItemStatCombatPreview01/[QA Only] Run ItemStat Combat Preview";

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                BattleSandboxBuildCombatPreviewValidator.Validate(),
                Validate()
            };
        }

        public static BattleSandboxBuildCombatPreview BuildDefaultPreview()
        {
            return BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreview();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BuildSandbox ItemStat Combat Preview 01");
            BattleSandboxBuildCombatPreview preview = BuildDefaultPreview();
            ValidateIsolation(report, preview);
            ValidateItemStatFeedback(report, preview);
            ValidateTaomuSwordFeedback(report, preview);
            return report;
        }

        public static IReadOnlyList<BattleSandboxBuildCombatPreviewRow> ItemStatRows(
            BattleSandboxBuildCombatPreview preview)
        {
            IReadOnlyList<BattleSandboxBuildCombatPreviewRow> rows = preview?.rows?
                .Where(row => row != null
                    && !string.IsNullOrWhiteSpace(row.sourceDataPath)
                    && row.sourceDataPath.IndexOf(".itemStat", StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();
            return rows ?? Array.Empty<BattleSandboxBuildCombatPreviewRow>();
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            if (preview == null)
            {
                report.AddError("ITEM_STAT_COMBAT_PREVIEW_NULL", "Default BuildSandbox preview was not created.", PackageName);
                return;
            }

            if (!preview.devOnly || preview.isEnabled)
            {
                report.AddError(
                    "ITEM_STAT_COMBAT_SCOPE_LEAK",
                    "Preview must stay devOnly=true and isEnabled=false.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            if (!preview.calculatesItemStatCombatPreview)
            {
                report.AddError(
                    "ITEM_STAT_COMBAT_FLAG_FALSE",
                    "Build combat preview must explicitly calculate ItemStat combat preview rows.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "ITEM_STAT_COMBAT_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            int leakCount = (preview.FeatureFlagDefaultTrueCount
                    + preview.FormalFlowLeakCount
                    + preview.PlayerSideAnswerLeakCount
                    + preview.ItemStatScopeLeakCount);
            if (leakCount != 0)
            {
                report.AddError(
                    "ITEM_STAT_COMBAT_LEAK_COUNT_NONZERO",
                    $"ItemStat combat preview must stay leak-free. actual={leakCount}.",
                    nameof(BattleSandboxBuildCombatPreview));
            }
        }

        private static void ValidateItemStatFeedback(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            if ((preview?.ItemStatProfileCount ?? 0) <= 0)
            {
                report.AddError(
                    "ITEM_STAT_COMBAT_PROFILE_COUNT_ZERO",
                    "Build combat preview must read ItemStat profiles from placed snapshots.",
                    nameof(BuildSandboxPlacedItemSnapshot));
            }

            IReadOnlyList<BattleSandboxBuildCombatPreviewRow> rows = ItemStatRows(preview);
            if (rows.Count < 3)
            {
                report.AddError(
                    "ITEM_STAT_COMBAT_ROW_COUNT_LOW",
                    $"Expected at least 3 ItemStat combat feedback rows. actual={rows.Count}.",
                    nameof(BattleSandboxBuildCombatPreviewRow));
                return;
            }

            foreach (BattleSandboxBuildCombatPreviewRow row in rows)
            {
                if (row.playerSideAnswerLeak)
                {
                    report.AddError(
                        "ITEM_STAT_COMBAT_PLAYER_LEAK",
                        $"ItemStat feedback must not leak answer tokens or Latin player text. id={row.feedbackId}.",
                        nameof(BattleSandboxBuildCombatPreviewRow));
                }

                if (row.formalFlowLeak)
                {
                    report.AddError(
                        "ITEM_STAT_COMBAT_FORMAL_LEAK",
                        $"ItemStat feedback must not connect formal combat, saves, rewards, chapters, or flags. id={row.feedbackId}.",
                        nameof(BattleSandboxBuildCombatPreviewRow));
                }

                if (!string.Equals(row.developerDataPanelFieldKey, "itemStatCombatPreview", StringComparison.Ordinal))
                {
                    report.AddError(
                        "ITEM_STAT_COMBAT_DEVELOPER_KEY_MISMATCH",
                        $"ItemStat feedback row must use itemStatCombatPreview developer key. id={row.feedbackId}.",
                        nameof(BattleSandboxBuildCombatPreviewRow));
                }
            }

            report.AddInfo(
                "ITEM_STAT_COMBAT_ROWS_READY",
                $"ItemStat combat feedback rows present: {rows.Count}.",
                nameof(BattleSandboxBuildCombatPreviewRow));
        }

        private static void ValidateTaomuSwordFeedback(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            BattleSandboxBuildCombatPreviewRow row = ItemStatRows(preview)
                .FirstOrDefault(candidate =>
                    string.Equals(candidate.feedbackId, "itemStat.taomuSword", StringComparison.Ordinal));
            if (row == null)
            {
                report.AddError(
                    "ITEM_STAT_TAOMU_FEEDBACK_MISSING",
                    "Taomu sword ItemStat must produce a masked combat feedback row.",
                    nameof(BattleSandboxBuildCombatPreviewRow));
                return;
            }

            if (!string.Equals(row.feedbackKind, BattleSandboxEnemyCombatFeedbackKinds.WeaknessWindow, StringComparison.Ordinal))
            {
                report.AddError(
                    "ITEM_STAT_TAOMU_FEEDBACK_KIND_MISMATCH",
                    $"Taomu sword feedback should appear as weakness-window cue. actual={row.feedbackKind}.",
                    nameof(BattleSandboxBuildCombatPreviewRow));
            }

            report.AddInfo(
                "ITEM_STAT_TAOMU_FEEDBACK_READY",
                "Taomu sword ItemStat is connected to a masked combat feedback row.",
                nameof(BattleSandboxBuildCombatPreviewRow));
        }
    }
}
#endif
