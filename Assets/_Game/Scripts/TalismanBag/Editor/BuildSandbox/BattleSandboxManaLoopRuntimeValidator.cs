#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxManaLoopRuntimeValidator
    {
        public const string PackageName = BattleSandboxManaLoopPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxManaLoopRuntime01/[QA Only] Run Mana Loop Runtime";

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxItemStatFoundationValidator.Validate(),
                Validate()
            };
        }

        public static BattleSandboxManaLoopPreview BuildDefaultPreview()
        {
            return BattleSandboxManaLoopPreviewBuilder.BuildDefaultPreview();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattleSandbox ManaLoop Runtime 01");
            BattleSandboxManaLoopPreview preview = BuildDefaultPreview();
            ValidateIsolation(report, preview);
            ValidateRows(report, preview);
            ValidateItemStatSource(report, preview);
            return report;
        }

        public static IReadOnlyList<BattleSandboxManaLoopPreviewRow> Rows(
            BattleSandboxManaLoopPreview preview)
        {
            if (preview?.rows == null)
            {
                return Array.Empty<BattleSandboxManaLoopPreviewRow>();
            }

            return preview.rows;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxManaLoopPreview preview)
        {
            if (preview == null)
            {
                report.AddError("MANA_LOOP_PREVIEW_NULL", "Mana loop preview was not created.", PackageName);
                return;
            }

            if (!preview.devOnly || preview.isEnabled)
            {
                report.AddError(
                    "MANA_LOOP_SCOPE_LEAK",
                    "Mana loop preview must stay devOnly=true and isEnabled=false.",
                    nameof(BattleSandboxManaLoopPreview));
            }

            if (!preview.readsBuildSandboxItemStat)
            {
                report.AddError(
                    "MANA_LOOP_ITEM_STAT_READ_FALSE",
                    "Mana loop must explicitly read V0.4 BuildSandbox ItemStat.",
                    nameof(BattleSandboxManaLoopPreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "MANA_LOOP_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if (preview.FormalLeakCount != 0)
            {
                report.AddError(
                    "MANA_LOOP_FORMAL_LEAK_NONZERO",
                    $"Mana loop must not touch formal combat, saves, rewards, Boss, or chapters. actual={preview.FormalLeakCount}.",
                    nameof(BattleSandboxManaLoopPreview));
            }

            if (preview.UiLayoutWriteCount != 0 || preview.touchesFormalSceneUiLayout)
            {
                report.AddError(
                    "MANA_LOOP_UI_LAYOUT_WRITE",
                    "Mana loop may update text/fill/runtime floating text only; RectTransform/layout writes must stay zero.",
                    nameof(BattleSandboxManaLoopPreview));
            }
        }

        private static void ValidateRows(
            BuildSandboxValidationReport report,
            BattleSandboxManaLoopPreview preview)
        {
            IReadOnlyList<BattleSandboxManaLoopPreviewRow> rows = Rows(preview);
            if (rows.Count == 0)
            {
                report.AddError("MANA_LOOP_ROWS_EMPTY", "Mana loop preview rows are empty.", PackageName);
                return;
            }

            if ((preview?.ManaGainRowCount ?? 0) <= 0)
            {
                report.AddError(
                    "MANA_LOOP_GAIN_ROWS_MISSING",
                    "Mana loop must produce at least one mana gain row.",
                    nameof(BattleSandboxManaLoopPreviewRow));
            }

            if ((preview?.ManaSpendRowCount ?? 0) <= 0)
            {
                report.AddError(
                    "MANA_LOOP_SPEND_ROWS_MISSING",
                    "Mana loop must produce at least one mana spend row.",
                    nameof(BattleSandboxManaLoopPreviewRow));
            }

            if ((preview?.generatedManaTotal ?? 0) <= 0)
            {
                report.AddError(
                    "MANA_LOOP_GENERATED_TOTAL_ZERO",
                    "Mana loop generated mana total must be positive.",
                    nameof(BattleSandboxManaLoopPreview));
            }

            if ((preview?.spentManaTotal ?? 0) <= 0)
            {
                report.AddError(
                    "MANA_LOOP_SPENT_TOTAL_ZERO",
                    "Mana loop spent mana total must be positive.",
                    nameof(BattleSandboxManaLoopPreview));
            }

            foreach (BattleSandboxManaLoopPreviewRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("MANA_LOOP_ROW_NULL", "Mana loop row is null.", nameof(BattleSandboxManaLoopPreviewRow));
                    continue;
                }

                if (row.playerSideAnswerLeak)
                {
                    report.AddError(
                        "MANA_LOOP_PLAYER_LEAK",
                        $"Mana loop row must not leak answer tokens. id={row.rowId}.",
                        nameof(BattleSandboxManaLoopPreviewRow));
                }

                if (row.formalFlowLeak)
                {
                    report.AddError(
                        "MANA_LOOP_ROW_FORMAL_LEAK",
                        $"Mana loop row must not connect formal combat/data/chapter surfaces. id={row.rowId}.",
                        nameof(BattleSandboxManaLoopPreviewRow));
                }

                if (!string.Equals(row.developerDataPanelFieldKey, BattleSandboxManaLoopPreview.DeveloperDataPanelFieldKey, StringComparison.Ordinal))
                {
                    report.AddError(
                        "MANA_LOOP_DEVELOPER_KEY_MISMATCH",
                        $"Mana loop row must use {BattleSandboxManaLoopPreview.DeveloperDataPanelFieldKey}. id={row.rowId}.",
                        nameof(BattleSandboxManaLoopPreviewRow));
                }
            }

            report.AddInfo(
                "MANA_LOOP_ROWS_READY",
                $"Mana loop rows present: {rows.Count}; gain={preview?.ManaGainRowCount ?? 0}, spend={preview?.ManaSpendRowCount ?? 0}.",
                nameof(BattleSandboxManaLoopPreviewRow));
        }

        private static void ValidateItemStatSource(
            BuildSandboxValidationReport report,
            BattleSandboxManaLoopPreview preview)
        {
            if ((preview?.sourceItemStatProfileCount ?? 0) <= 0)
            {
                report.AddError(
                    "MANA_LOOP_ITEM_STAT_PROFILE_ZERO",
                    "Mana loop must read ItemStat profiles from placed snapshots.",
                    nameof(BuildSandboxItemStat));
            }

            foreach (BattleSandboxManaLoopPreviewRow row in Rows(preview))
            {
                if (row == null)
                {
                    continue;
                }

                string sourceDataPath = row.sourceDataPath ?? string.Empty;
                if (!row.readsBuildSandboxItemStat
                    || sourceDataPath.IndexOf("BuildSandboxLayoutSnapshot.placedItems[]", StringComparison.Ordinal) < 0
                    || sourceDataPath.IndexOf(".itemStat.", StringComparison.Ordinal) < 0)
                {
                    report.AddError(
                        "MANA_LOOP_SOURCE_NOT_ITEM_STAT",
                        $"Mana loop source must be V0.4 BuildSandbox ItemStat only. id={row.rowId}, source={sourceDataPath}.",
                        nameof(BattleSandboxManaLoopPreviewRow));
                }
            }
        }
    }
}
#endif
