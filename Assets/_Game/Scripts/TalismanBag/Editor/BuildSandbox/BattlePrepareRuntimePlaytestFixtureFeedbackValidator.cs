#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattlePrepareRuntimePlaytestFixtureFeedbackValidator
    {
        public const string PackageName = BattlePrepareComponentAdapterRuntimePlaytestPlan.FixtureFeedbackPackageName;
        private const int RequiredFixtureItemCount = 4;
        private const int RequiredFeedbackCount = 5;

        private static readonly Dictionary<string, (int cellCount, bool rotationAllowed)> RequiredShapes = new()
        {
            { "Single1", (1, false) },
            { "Vertical2", (2, true) },
            { "Corner3", (3, true) },
            { "Square4", (4, false) }
        };

        private static readonly string[] RequiredFeedbackIds =
        {
            "rotation",
            "occupancyPreview",
            "validPlacement",
            "outOfGrid",
            "overlap"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareComponentAdapterRuntimePlaytestValidator.BuildValidationReports();
            reports.Add(Validate());
            return reports;
        }

        public static BattlePrepareComponentAdapterRuntimePlaytestPlan BuildDefaultPlan()
        {
            return BattlePrepareComponentAdapterRuntimePlaytestPlan.BuildDefault();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report =
                new("BattlePrepare Runtime Playtest Fixture Feedback Fix 01");
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = BuildDefaultPlan();

            ValidatePlanIsolation(report, plan);
            ValidateFixtureItems(report, plan);
            ValidateFixtureFeedback(report, plan);
            return report;
        }

        private static void ValidatePlanIsolation(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                report.AddError("BATTLE_PREPARE_FIXTURE_PLAN_NULL", "Fixture feedback plan was not created.", PackageName);
                return;
            }

            if (!plan.devOnly || !plan.fixtureProviderDevOnly)
            {
                report.AddError(
                    "BATTLE_PREPARE_FIXTURE_DEVONLY_FALSE",
                    "Runtime fixture provider must remain devOnly.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_ENABLED_TRUE", plan.isEnabled);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_WRITES_FORMAL_SCENE", plan.writesFormalScene);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_WRITES_FORMAL_UI", plan.writesFormalUi);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_WRITES_FORMAL_POOL", plan.writesFixtureToFormalPool);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_WRITES_SAVE", plan.writesFixtureToSaveData);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_TOUCHES_RUNFLOW", plan.touchesRunFlow);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_TOUCHES_PAGESTATE", plan.touchesPageState);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_TOUCHES_FORMATIONSTATE", plan.touchesFormationState);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_TOUCHES_SAVE", plan.touchesSaveData);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_TOUCHES_BOSS_REWARD_NUMERIC", plan.touchesBossRewardDropNumeric);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_REDRAWS_BOARD", plan.redrewV04Board);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_REDRAWS_TRAY", plan.redrewV04ItemTray);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_REWRITES_DRAG", plan.rewroteDragFeel);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_REWRITES_PULLUP", plan.rewrotePullUpAnimation);
            ValidateFalse(report, "BATTLE_PREPARE_FIXTURE_FEATURE_FLAG_TRUE", plan.featureFlagDefaultEnabled);

            if (!plan.createsRuntimeFixtureProvider
                || !plan.fixtureDefinitionsDontSave
                || !plan.injectsFixtureItemsIntoMatureTray
                || !plan.fixtureItemsVisibleInTray)
            {
                report.AddError(
                    "BATTLE_PREPARE_FIXTURE_PROVIDER_INCOMPLETE",
                    "Fixture provider must create DontSave visible items in the mature tray.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
                return;
            }

            report.AddInfo(
                "BATTLE_PREPARE_FIXTURE_ISOLATION_PASS",
                "Fixture feedback provider is declared devOnly, DontSave, mature-tray-only, and disconnected from formal pools/save.",
                nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
        }

        private static void ValidateFixtureItems(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            IReadOnlyList<BattlePrepareRuntimeFixtureItemRow> fixtures =
                plan != null && plan.fixtureItems != null
                    ? plan.fixtureItems
                    : Array.Empty<BattlePrepareRuntimeFixtureItemRow>();
            ValidateMinimum(
                report,
                "BATTLE_PREPARE_FIXTURE_ITEM_COUNT",
                "Fixture item",
                fixtures.Count,
                RequiredFixtureItemCount);

            HashSet<string> seenShapeIds = new(StringComparer.Ordinal);
            foreach (BattlePrepareRuntimeFixtureItemRow row in fixtures)
            {
                if (row == null)
                {
                    report.AddError("BATTLE_PREPARE_FIXTURE_ITEM_NULL", "Null fixture item row.", PackageName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.itemId)
                    || string.IsNullOrWhiteSpace(row.displayName)
                    || string.IsNullOrWhiteSpace(row.shapeId)
                    || string.IsNullOrWhiteSpace(row.manualProbe))
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_ITEM_INCOMPLETE",
                        $"Fixture item row is incomplete. shape={row.shapeId}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                }

                if (!RequiredShapes.TryGetValue(row.shapeId, out (int cellCount, bool rotationAllowed) required))
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_ITEM_UNKNOWN_SHAPE",
                        $"Unexpected fixture shape: {row.shapeId}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                    continue;
                }

                seenShapeIds.Add(row.shapeId);
                if (row.cellCount != required.cellCount)
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_ITEM_CELL_COUNT",
                        $"Fixture cell count mismatch. shape={row.shapeId}, actual={row.cellCount}, expected={required.cellCount}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                }

                if (row.rotationAllowed != required.rotationAllowed)
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_ITEM_ROTATION",
                        $"Fixture rotation flag mismatch. shape={row.shapeId}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                }

                if (!row.trayVisible)
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_ITEM_NOT_VISIBLE",
                        $"Fixture must be visible in the mature tray. shape={row.shapeId}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                }

                if (!row.devOnly || row.isEnabled || row.canDrop || row.writesFormalConfig || row.writesSaveData)
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_ITEM_ISOLATION_LEAK",
                        $"Fixture item isolation leak. shape={row.shapeId}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                }
            }

            foreach (string requiredShape in RequiredShapes.Keys)
            {
                if (!seenShapeIds.Contains(requiredShape))
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_ITEM_REQUIRED_MISSING",
                        $"Missing required runtime fixture shape: {requiredShape}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                }
                else
                {
                    report.AddInfo(
                        "BATTLE_PREPARE_FIXTURE_ITEM_REQUIRED_PRESENT",
                        $"Required runtime fixture shape present: {requiredShape}.",
                        nameof(BattlePrepareRuntimeFixtureItemRow));
                }
            }
        }

        private static void ValidateFixtureFeedback(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            IReadOnlyList<BattlePrepareRuntimeFixtureFeedbackRow> feedbackRows =
                plan != null && plan.fixtureFeedback != null
                    ? plan.fixtureFeedback
                    : Array.Empty<BattlePrepareRuntimeFixtureFeedbackRow>();
            ValidateMinimum(
                report,
                "BATTLE_PREPARE_FIXTURE_FEEDBACK_COUNT",
                "Fixture feedback",
                feedbackRows.Count,
                RequiredFeedbackCount);

            HashSet<string> seenFeedbackIds = new(StringComparer.Ordinal);
            foreach (BattlePrepareRuntimeFixtureFeedbackRow row in feedbackRows)
            {
                if (row == null)
                {
                    report.AddError("BATTLE_PREPARE_FIXTURE_FEEDBACK_NULL", "Null fixture feedback row.", PackageName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.feedbackId)
                    || string.IsNullOrWhiteSpace(row.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(row.trigger)
                    || string.IsNullOrWhiteSpace(row.expectedChineseFeedback))
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_FEEDBACK_INCOMPLETE",
                        $"Fixture feedback row is incomplete. feedback={row.feedbackId}.",
                        nameof(BattlePrepareRuntimeFixtureFeedbackRow));
                }

                if (!row.required)
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_FEEDBACK_NOT_REQUIRED",
                        $"Fixture feedback row must be required. feedback={row.feedbackId}.",
                        nameof(BattlePrepareRuntimeFixtureFeedbackRow));
                }

                seenFeedbackIds.Add(row.feedbackId);
            }

            foreach (string requiredFeedback in RequiredFeedbackIds)
            {
                if (!seenFeedbackIds.Contains(requiredFeedback))
                {
                    report.AddError(
                        "BATTLE_PREPARE_FIXTURE_FEEDBACK_REQUIRED_MISSING",
                        $"Missing required runtime feedback target: {requiredFeedback}.",
                        nameof(BattlePrepareRuntimeFixtureFeedbackRow));
                }
                else
                {
                    report.AddInfo(
                        "BATTLE_PREPARE_FIXTURE_FEEDBACK_REQUIRED_PRESENT",
                        $"Required runtime feedback target present: {requiredFeedback}.",
                        nameof(BattlePrepareRuntimeFixtureFeedbackRow));
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
                report.AddError(code, "This fixture feedback isolation flag must stay false.", PackageName);
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
