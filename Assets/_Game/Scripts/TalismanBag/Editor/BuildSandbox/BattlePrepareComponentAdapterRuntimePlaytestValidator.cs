#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattlePrepareComponentAdapterRuntimePlaytestValidator
    {
        public const string PackageName = BattlePrepareComponentAdapterRuntimePlaytestPlan.PackageName;
        private const int RequiredRuntimeSurfaceCount = 6;
        private const int RequiredUserStepCount = 6;

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareComponentAdapterPlaytestValidator.BuildValidationReports();
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
                new("BattlePrepare Component Adapter Runtime Playtest 01");
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan = BuildDefaultPlan();

            ValidateIsolation(report, plan);
            ValidateRuntimeTruth(report, plan);
            ValidateMenuAndScene(report, plan);
            ValidateRuntimeSurfaces(report, plan);
            ValidateUserSteps(report, plan);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                report.AddError("BATTLE_PREPARE_RUNTIME_PLAYTEST_NULL", "Runtime playtest plan was not created.", PackageName);
                return;
            }

            if (!string.Equals(plan.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={plan.packageName}.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            if (!plan.devOnly)
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_DEVONLY_FALSE",
                    "Runtime playtest must remain devOnly=true.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_ENABLED_TRUE", plan.isEnabled);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_WRITES_FORMAL_SCENE", plan.writesFormalScene);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_WRITES_FORMAL_UI", plan.writesFormalUi);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_OVERWRITES_RECTTRANSFORM", plan.overwritesHandTunedRectTransform);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_TOUCHES_RUNFLOW", plan.touchesRunFlow);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_TOUCHES_PAGESTATE", plan.touchesPageState);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_TOUCHES_FORMATIONSTATE", plan.touchesFormationState);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_TOUCHES_SAVE", plan.touchesSaveData);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_TOUCHES_BOSS_REWARD_NUMERIC", plan.touchesBossRewardDropNumeric);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_REDRAWS_V04_BOARD", plan.redrewV04Board);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_REDRAWS_V04_TRAY", plan.redrewV04ItemTray);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_REWRITES_DRAG", plan.rewroteDragFeel);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_REWRITES_PULLUP", plan.rewrotePullUpAnimation);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_PROMOTES_TEMP_UI", plan.promotesTemporaryPreviewUi);
            ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_FEATURE_FLAG_TRUE", plan.featureFlagDefaultEnabled);

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_FEATURE_FLAG_DEFAULT_TRUE",
                    "All BuildSandbox FeatureFlags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_ISOLATION_PASS",
                    "Runtime playtest is devOnly, manual, disabled by default, and disconnected from formal flow/save/battle surfaces.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }
        }

        private static void ValidateRuntimeTruth(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return;
            }

            if (!plan.manualOnly)
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_MANUAL_ONLY_FALSE",
                    "Runtime playtest must be manual-only and never silently run in formal flow.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            if (!plan.realUnityPlayTouchablePath)
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_NO_REAL_PLAY_PATH",
                    "Assignment requires a real Unity Play touchable path.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            if (plan.staticReportOnly)
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_STATIC_ONLY",
                    "This package must not be static-report-only.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            if (plan.automationClaimsHandFeelPass)
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_FALSE_HAND_FEEL_CLAIM",
                    "Automation must not claim that human hand-feel passed.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            if (!plan.createsDontSaveRuntimeLauncher
                || !plan.opensFormalSceneReadOnly
                || !plan.callsMaturePrepareEntrypoint
                || !plan.usesMatureBoardTrayDragPullup)
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_TOUCH_PATH_INCOMPLETE",
                    "Runtime path must open the mature prepare surface and install only a DontSave extension launcher.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_REAL_PATH_DECLARED",
                    "Manual Unity Play path is declared and separated from automated hand-feel claims.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }
        }

        private static void ValidateMenuAndScene(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            if (plan == null)
            {
                return;
            }

            if (!string.Equals(plan.manualMenuPath, BattlePrepareComponentAdapterRuntimePlaytestPlan.ManualMenuPath, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(plan.manualMenuPath))
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_MANUAL_MENU_INVALID",
                    $"Manual menu path mismatch. actual={plan.manualMenuPath}.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            if (!string.Equals(plan.qaMenuPath, BattlePrepareComponentAdapterRuntimePlaytestPlan.QaMenuPath, StringComparison.Ordinal)
                || string.IsNullOrWhiteSpace(plan.qaMenuPath))
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_QA_MENU_INVALID",
                    $"QA menu path mismatch. actual={plan.qaMenuPath}.",
                    nameof(BattlePrepareComponentAdapterRuntimePlaytestPlan));
            }

            SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(plan.targetScenePath);
            if (scene == null)
            {
                report.AddError(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_TARGET_SCENE_MISSING",
                    $"Target scene is missing: {plan.targetScenePath}.",
                    plan.targetScenePath);
            }
            else
            {
                report.AddInfo(
                    "BATTLE_PREPARE_RUNTIME_PLAYTEST_TARGET_SCENE_FOUND",
                    $"Target scene exists: {plan.targetScenePath}.",
                    plan.targetScenePath);
            }
        }

        private static void ValidateRuntimeSurfaces(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            IReadOnlyList<BattlePrepareComponentAdapterRuntimeSurfaceRow> surfaces =
                plan != null && plan.runtimeSurfaces != null
                    ? plan.runtimeSurfaces
                    : Array.Empty<BattlePrepareComponentAdapterRuntimeSurfaceRow>();
            ValidateMinimum(
                report,
                "BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_COUNT",
                "Runtime surface",
                surfaces.Count,
                RequiredRuntimeSurfaceCount);

            foreach (BattlePrepareComponentAdapterRuntimeSurfaceRow row in surfaces)
            {
                if (row == null)
                {
                    report.AddError("BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_NULL", "Null runtime surface row.", PackageName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.surfaceId)
                    || string.IsNullOrWhiteSpace(row.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(row.matureSource)
                    || string.IsNullOrWhiteSpace(row.runtimeEvidence))
                {
                    report.AddError(
                        "BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_INCOMPLETE",
                        $"Runtime surface row is incomplete. surface={row.surfaceId}.",
                        nameof(BattlePrepareComponentAdapterRuntimeSurfaceRow));
                }

                if (!row.touchableInPlay)
                {
                    report.AddError(
                        "BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_NOT_TOUCHABLE",
                        $"Runtime surface must be touchable in Play. surface={row.surfaceId}.",
                        nameof(BattlePrepareComponentAdapterRuntimeSurfaceRow));
                }

                if (!row.extensionOnly)
                {
                    report.AddError(
                        "BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_NOT_EXTENSION_ONLY",
                        $"Runtime surface must be extension-only. surface={row.surfaceId}.",
                        nameof(BattlePrepareComponentAdapterRuntimeSurfaceRow));
                }

                ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_WRITES_UI", row.writesFormalUi);
                ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_WRITES_SCENE", row.modifiesFormalScene);
                ValidateFalse(report, "BATTLE_PREPARE_RUNTIME_PLAYTEST_SURFACE_OVERWRITES_RECT", row.overwritesRectTransform);
            }
        }

        private static void ValidateUserSteps(
            BuildSandboxValidationReport report,
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan)
        {
            IReadOnlyList<BattlePrepareComponentAdapterRuntimeStepRow> steps =
                plan != null && plan.userSteps != null
                    ? plan.userSteps
                    : Array.Empty<BattlePrepareComponentAdapterRuntimeStepRow>();
            ValidateMinimum(
                report,
                "BATTLE_PREPARE_RUNTIME_PLAYTEST_USER_STEP_COUNT",
                "User handtest step",
                steps.Count,
                RequiredUserStepCount);

            foreach (BattlePrepareComponentAdapterRuntimeStepRow row in steps)
            {
                if (row == null
                    || string.IsNullOrWhiteSpace(row.stepId)
                    || string.IsNullOrWhiteSpace(row.action)
                    || string.IsNullOrWhiteSpace(row.expected))
                {
                    report.AddError(
                        "BATTLE_PREPARE_RUNTIME_PLAYTEST_USER_STEP_INCOMPLETE",
                        "Each user step row needs id, action, and expected result.",
                        nameof(BattlePrepareComponentAdapterRuntimeStepRow));
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
                report.AddError(code, "This runtime playtest isolation flag must stay false.", PackageName);
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
