#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattlePrepareComponentAdapterPlaytestValidator
    {
        public const string PackageName = BattlePrepareComponentAdapterPlaytestController.PackageName;
        private const int RequiredProbeCount = 7;
        private const int RequiredManualCheckCount = 4;

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareComponentAdapterValidator.BuildValidationReports();
            reports.Add(Validate());
            return reports;
        }

        public static BattlePrepareComponentAdapterPlaytestController BuildDefaultPlaytest()
        {
            BattlePrepareComponentAdapter adapter =
                BattlePrepareComponentAdapterValidator.BuildDefaultAdapter();
            BattlePageViewSpec spec =
                BattlePageViewAdapterValidator.BuildDefaultAdapter().spec;
            return BattlePrepareComponentAdapterPlaytestController.Build(adapter, spec);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report =
                new("BattlePrepare Component Adapter Playtest 01");
            BattlePrepareComponentAdapterPlaytestController playtest =
                BuildDefaultPlaytest();

            ValidateIsolation(report, playtest);
            ValidateStaticPlaytestTruth(report, playtest);
            ValidateProbeRows(report, playtest);
            ValidateManualChecks(report, playtest);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterPlaytestController playtest)
        {
            if (playtest == null)
            {
                report.AddError("BATTLE_PREPARE_PLAYTEST_NULL", "Playtest controller was not created.", PackageName);
                return;
            }

            if (!string.Equals(playtest.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "BATTLE_PREPARE_PLAYTEST_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={playtest.packageName}.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }

            if (!playtest.devOnly)
            {
                report.AddError(
                    "BATTLE_PREPARE_PLAYTEST_DEVONLY_FALSE",
                    "Playtest must remain devOnly=true.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }

            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_ENABLED_TRUE", playtest.isEnabled);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_WRITES_FORMAL_SCENE", playtest.writesFormalScene);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_WRITES_FORMAL_UI", playtest.writesFormalUi);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_TOUCHES_PREPARE_CONTROLLER", playtest.touchesFormalPrepareController);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_OVERWRITES_RECTTRANSFORM", playtest.overwritesHandTunedRectTransform);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_TOUCHES_RUNFLOW", playtest.touchesRunFlow);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_TOUCHES_PAGESTATE", playtest.touchesPageState);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_TOUCHES_FORMATIONSTATE", playtest.touchesFormationState);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_TOUCHES_SAVE", playtest.touchesSaveData);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_TOUCHES_BOSS_REWARD_NUMERIC", playtest.touchesBossRewardDropNumeric);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_REDRAWS_V04_BOARD", playtest.redrewV04Board);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_REDRAWS_V04_TRAY", playtest.redrewV04ItemTray);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_REWRITES_DRAG", playtest.rewroteDragFeel);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_REWRITES_PULLUP", playtest.rewrotePullUpAnimation);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_PROMOTES_TEMP_UI", playtest.promotesTemporaryPreviewUi);
            ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_FEATURE_FLAG_TRUE", playtest.featureFlagDefaultEnabled);

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "BATTLE_PREPARE_PLAYTEST_FEATURE_FLAG_DEFAULT_TRUE",
                    "All BuildSandbox FeatureFlags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PREPARE_PLAYTEST_ISOLATION_PASS",
                    "Playtest is devOnly, disabled, and disconnected from formal UI/flow/save surfaces.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }
        }

        private static void ValidateStaticPlaytestTruth(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterPlaytestController playtest)
        {
            if (playtest == null)
            {
                return;
            }

            if (!playtest.staticPlaytestOnly)
            {
                report.AddError(
                    "BATTLE_PREPARE_PLAYTEST_STATIC_DISCLOSURE_MISSING",
                    "This package must truthfully disclose that it is static/report-only unless a real devOnly play session is executed.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }

            if (playtest.realRuntimeInteractionExecuted)
            {
                report.AddError(
                    "BATTLE_PREPARE_PLAYTEST_RUNTIME_EXECUTION_UNEXPECTED",
                    "This playtest package must not silently execute formal runtime interaction.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }

            if (string.IsNullOrWhiteSpace(playtest.validationMode)
                || playtest.validationMode.IndexOf("Static", StringComparison.OrdinalIgnoreCase) < 0)
            {
                report.AddError(
                    "BATTLE_PREPARE_PLAYTEST_MODE_INVALID",
                    $"Validation mode must mark static devOnly scope. actual={playtest.validationMode}.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PREPARE_PLAYTEST_STATIC_SCOPE_DISCLOSED",
                    "Report-only static hand-feel probe is explicitly disclosed.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }
        }

        private static void ValidateProbeRows(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterPlaytestController playtest)
        {
            IReadOnlyList<BattlePrepareComponentAdapterPlaytestProbeRow> probes =
                playtest != null && playtest.probes != null
                    ? playtest.probes
                    : Array.Empty<BattlePrepareComponentAdapterPlaytestProbeRow>();
            ValidateMinimum(report, "BATTLE_PREPARE_PLAYTEST_PROBE_COUNT", "Probe rows", probes.Count, RequiredProbeCount);

            foreach (BattlePrepareComponentAdapterPlaytestProbeRow row in probes)
            {
                if (row == null)
                {
                    report.AddError("BATTLE_PREPARE_PLAYTEST_PROBE_NULL", "Null probe row.", PackageName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.probeId)
                    || string.IsNullOrWhiteSpace(row.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(row.matureComponent)
                    || string.IsNullOrWhiteSpace(row.extensionId)
                    || string.IsNullOrWhiteSpace(row.adapterOutputKey))
                {
                    report.AddError(
                        "BATTLE_PREPARE_PLAYTEST_PROBE_INCOMPLETE",
                        $"Probe row is missing stable ids or display labels. probe={row.probeId}.",
                        nameof(BattlePrepareComponentAdapterPlaytestProbeRow));
                }

                if (!row.passed)
                {
                    report.AddError(
                        "BATTLE_PREPARE_PLAYTEST_PROBE_FAILED",
                        $"Probe did not pass. probe={row.probeId}, evidence={row.evidence}.",
                        nameof(BattlePrepareComponentAdapterPlaytestProbeRow));
                }

                ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_PROBE_WRITES_UI", row.writesFormalUi);
                ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_PROBE_WRITES_SCENE", row.modifiesFormalScene);
                ValidateFalse(report, "BATTLE_PREPARE_PLAYTEST_PROBE_OVERWRITES_RECT", row.overwritesRectTransform);
            }

            if (playtest != null && playtest.AllProbesPassed)
            {
                report.AddInfo(
                    "BATTLE_PREPARE_PLAYTEST_PROBES_PASS",
                    $"All playtest probes passed. passed={playtest.PassedProbeCount}/{playtest.ProbeCount}.",
                    nameof(BattlePrepareComponentAdapterPlaytestController));
            }
        }

        private static void ValidateManualChecks(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterPlaytestController playtest)
        {
            IReadOnlyList<BattlePrepareComponentAdapterPlaytestManualCheckRow> checks =
                playtest != null && playtest.manualChecks != null
                    ? playtest.manualChecks
                    : Array.Empty<BattlePrepareComponentAdapterPlaytestManualCheckRow>();
            ValidateMinimum(report, "BATTLE_PREPARE_PLAYTEST_MANUAL_CHECK_COUNT", "Manual check rows", checks.Count, RequiredManualCheckCount);

            foreach (BattlePrepareComponentAdapterPlaytestManualCheckRow row in checks)
            {
                if (row == null
                    || string.IsNullOrWhiteSpace(row.checkId)
                    || string.IsNullOrWhiteSpace(row.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(row.action)
                    || string.IsNullOrWhiteSpace(row.expected))
                {
                    report.AddError(
                        "BATTLE_PREPARE_PLAYTEST_MANUAL_CHECK_INCOMPLETE",
                        "Each manual check row needs id, Chinese display name, action, and expected result.",
                        nameof(BattlePrepareComponentAdapterPlaytestManualCheckRow));
                }
            }
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "This playtest isolation flag must stay false.", PackageName);
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
                report.AddError(code, $"{label} too low. actual={actual}, expected>={expected}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} pass. actual={actual}, expected>={expected}.", PackageName);
        }
    }
}
#endif
