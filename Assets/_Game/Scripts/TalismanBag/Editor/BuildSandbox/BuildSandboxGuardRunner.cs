#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BuildSandboxGuardRunner
    {
        public static void RunBuildSandboxConfigPanelMenu()
        {
            RunBuildSandboxConfigPanel(throwOnFailure: false);
        }
        public static void RunBattleSandboxPreviewSceneMenu()
        {
            RunBattleSandboxPreviewScene(throwOnFailure: false);
        }
        public static void RunBuildSandboxPreviewContextMenu()
        {
            RunBuildSandboxPreviewContext(throwOnFailure: false);
        }
        public static void RunBattlePageViewAdapterMenu()
        {
            RunBattlePageViewAdapter(throwOnFailure: false);
        }
        public static void RunBuildGridInteractionPreviewMenu()
        {
            RunBuildGridInteractionPreview(throwOnFailure: false);
        }
        public static void RunBattlePrepareComponentAdapterMenu()
        {
            RunBattlePrepareComponentAdapter(throwOnFailure: false);
        }
        public static void RunBattlePrepareComponentAdapterPlaytestMenu()
        {
            RunBattlePrepareComponentAdapterPlaytest(throwOnFailure: false);
        }
        public static void OpenBattlePrepareComponentAdapterRuntimePlaytestMenu()
        {
            BattlePrepareComponentAdapterRuntimePlaytestLauncher.OpenRuntimePlaytest();
        }
        public static void RunBattlePrepareComponentAdapterRuntimePlaytestMenu()
        {
            RunBattlePrepareComponentAdapterRuntimePlaytest(throwOnFailure: false);
        }
        public static void RunBattlePrepareRuntimePlaytestFixtureFeedbackMenu()
        {
            RunBattlePrepareRuntimePlaytestFixtureFeedback(throwOnFailure: false);
        }
        public static void RunShapeAwareItemTrayFixtureMenu()
        {
            RunShapeAwareItemTrayFixture(throwOnFailure: false);
        }
        public static void RunShapeAwareTrayPackingDragMenu()
        {
            RunShapeAwareTrayPackingDrag(throwOnFailure: false);
        }
        public static void RunShapePlacementSessionMenu()
        {
            RunShapePlacementSession(throwOnFailure: false);
        }
        public static void RunShapeAwareItemTrayGridMenu()
        {
            RunShapeAwareItemTrayGrid(throwOnFailure: false);
        }
        public static void RunMobileShapePlacementMenu()
        {
            RunMobileShapePlacement(throwOnFailure: false);
        }
        public static void RunMobileShapePlacementRuntimeIntegrationMenu()
        {
            RunMobileShapePlacementRuntimeIntegration(throwOnFailure: false);
        }
        public static void RunBattlePrepareExtensionSeamMenu()
        {
            RunBattlePrepareExtensionSeam(throwOnFailure: false);
        }
        public static void RunBuildTuningDataPanelPreviewMenu()
        {
            RunBuildTuningDataPanelPreview(throwOnFailure: false);
        }
        public static void RunMechanicHintFeedbackPreviewMenu()
        {
            RunMechanicHintFeedbackPreview(throwOnFailure: false);
        }
        public static void RunBattleSandboxEnemyEncounterPreviewMenu()
        {
            RunBattleSandboxEnemyEncounterPreview(throwOnFailure: false);
        }
        public static void RunBattleSandboxEnemyCombatFeedbackUiReuseMenu()
        {
            RunBattleSandboxEnemyCombatFeedbackUiReuse(throwOnFailure: false);
        }
        public static void RunBattleSandboxBuildCombatPreviewMenu()
        {
            RunBattleSandboxBuildCombatPreview(throwOnFailure: false);
        }
        public static void RunBuildSandboxItemStatFoundationMenu()
        {
            RunBuildSandboxItemStatFoundation(throwOnFailure: false);
        }
        public static void RunBuildSandboxItemIdentityFamilyCorrectionMenu()
        {
            RunBuildSandboxItemIdentityFamilyCorrection(throwOnFailure: false);
        }
        public static void RunBuildSandboxItemStatCombatPreviewMenu()
        {
            RunBuildSandboxItemStatCombatPreview(throwOnFailure: false);
        }
        public static void RunBattleSandboxManaLoopRuntimeMenu()
        {
            RunBattleSandboxManaLoopRuntime(throwOnFailure: false);
        }
        public static void RunBattleSandboxRuntimeLoopMenu()
        {
            RunBattleSandboxRuntimeLoop(throwOnFailure: false);
        }
        public static void RunBattleSandboxItemEffectRuntimePreviewMenu()
        {
            RunBattleSandboxItemEffectRuntimePreview(throwOnFailure: false);
        }
        public static void RunFormationCorePowerRangeMenu()
        {
            RunFormationCorePowerRange(throwOnFailure: false);
        }
        public static void RunBattleSandboxPlayableLoopMenu()
        {
            RunBattleSandboxPlayableLoop(throwOnFailure: false);
        }
        public static void RunBuildSandboxPlayableRegressionMenu()
        {
            RunBuildSandboxPlayableRegression(throwOnFailure: false);
        }
        public static void RunBattleSandboxPlayableFullRosterRegressionMenu()
        {
            RunBattleSandboxPlayableFullRosterRegression(throwOnFailure: false);
        }
        public static void RunBattleSandboxDevChapterPlayableMenu()
        {
            RunBattleSandboxDevChapterPlayable(throwOnFailure: false);
        }

        public static void RunBuildSandboxConfigPanelBatch()
        {
            bool passed = RunBuildSandboxConfigPanel(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxPreviewSceneBatch()
        {
            bool passed = RunBattleSandboxPreviewScene(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildSandboxPreviewContextBatch()
        {
            bool passed = RunBuildSandboxPreviewContext(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattlePageViewAdapterBatch()
        {
            bool passed = RunBattlePageViewAdapter(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildGridInteractionPreviewBatch()
        {
            bool passed = RunBuildGridInteractionPreview(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattlePrepareComponentAdapterBatch()
        {
            bool passed = RunBattlePrepareComponentAdapter(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattlePrepareComponentAdapterPlaytestBatch()
        {
            bool passed = RunBattlePrepareComponentAdapterPlaytest(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattlePrepareComponentAdapterRuntimePlaytestBatch()
        {
            bool passed = RunBattlePrepareComponentAdapterRuntimePlaytest(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunShapeAwareTrayPackingDragBatch()
        {
            bool passed = RunShapeAwareTrayPackingDrag(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunShapePlacementSessionBatch()
        {
            bool passed = RunShapePlacementSession(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunShapeAwareItemTrayGridBatch()
        {
            bool passed = RunShapeAwareItemTrayGrid(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunMobileShapePlacementBatch()
        {
            bool passed = RunMobileShapePlacement(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunMobileShapePlacementRuntimeIntegrationBatch()
        {
            bool passed = RunMobileShapePlacementRuntimeIntegration(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattlePrepareExtensionSeamBatch()
        {
            bool passed = RunBattlePrepareExtensionSeam(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildTuningDataPanelPreviewBatch()
        {
            bool passed = RunBuildTuningDataPanelPreview(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunMechanicHintFeedbackPreviewBatch()
        {
            bool passed = RunMechanicHintFeedbackPreview(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxEnemyEncounterPreviewBatch()
        {
            bool passed = RunBattleSandboxEnemyEncounterPreview(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxEnemyCombatFeedbackUiReuseBatch()
        {
            bool passed = RunBattleSandboxEnemyCombatFeedbackUiReuse(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxBuildCombatPreviewBatch()
        {
            bool passed = RunBattleSandboxBuildCombatPreview(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildSandboxItemStatFoundationBatch()
        {
            bool passed = RunBuildSandboxItemStatFoundation(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildSandboxItemIdentityFamilyCorrectionBatch()
        {
            bool passed = RunBuildSandboxItemIdentityFamilyCorrection(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildSandboxItemStatCombatPreviewBatch()
        {
            bool passed = RunBuildSandboxItemStatCombatPreview(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxManaLoopRuntimeBatch()
        {
            bool passed = RunBattleSandboxManaLoopRuntime(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxRuntimeLoopBatch()
        {
            bool passed = RunBattleSandboxRuntimeLoop(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxItemEffectRuntimePreviewBatch()
        {
            bool passed = RunBattleSandboxItemEffectRuntimePreview(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunFormationCorePowerRangeBatch()
        {
            bool passed = RunFormationCorePowerRange(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxPlayableLoopBatch()
        {
            bool passed = RunBattleSandboxPlayableLoop(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildSandboxPlayableRegressionBatch()
        {
            bool passed = RunBuildSandboxPlayableRegression(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxPlayableFullRosterRegressionBatch()
        {
            bool passed = RunBattleSandboxPlayableFullRosterRegression(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBattleSandboxDevChapterPlayableBatch()
        {
            bool passed = RunBattleSandboxDevChapterPlayable(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static bool RunBuildSandboxConfigPanel(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxConfigPanelValidator.BuildValidationReports();

            string[] reportPaths = BuildSandboxConfigPanelReportWriter.WriteReports(reports);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BuildSandboxConfigPanel01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BuildSandboxConfigPanel01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattleSandboxPreviewScene(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                BuildProblemRulePoolValidator.Validate(),
                BuildProblemSeedDataValidator.Validate(),
                BuildSandboxConfigPanelValidator.Validate(),
                BattleSandboxPreviewSceneVerifier.Validate()
            };

            string[] reportPaths = BattleSandboxPreviewSceneReportWriter.WriteReports(reports);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattleSandboxPreviewScene01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattleSandboxPreviewScene01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildSandboxPreviewContext(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxPreviewContextValidator.BuildValidationReports();
            BuildSandboxPreviewContext context =
                BuildSandboxPreviewContextValidator.BuildDefaultContext();
            string[] reportPaths = BuildSandboxPreviewContextReportWriter.WriteReports(reports, context);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BuildSandboxPreviewContext01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BuildSandboxPreviewContext01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattlePageViewAdapter(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattlePageViewAdapterValidator.BuildValidationReports();
            BattlePageViewAdapter adapter =
                BattlePageViewAdapterValidator.BuildDefaultAdapter();
            string[] reportPaths = BattlePageViewAdapterReportWriter.WriteReports(reports, adapter);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattlePageViewAdapter01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattlePageViewAdapter01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildGridInteractionPreview(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BuildGridInteractionPreviewValidator.BuildValidationReports();
            string[] reportPaths = BuildGridInteractionPreviewReportWriter.WriteReports(reports);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BuildGridInteractionPreview01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BuildGridInteractionPreview01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattlePrepareComponentAdapter(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareComponentAdapterValidator.BuildValidationReports();
            BattlePrepareComponentAdapter adapter =
                BattlePrepareComponentAdapterValidator.BuildDefaultAdapter();
            string[] reportPaths =
                BattlePrepareComponentAdapterReportWriter.WriteReports(reports, adapter);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattlePrepareComponentAdapter01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattlePrepareComponentAdapter01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattlePrepareComponentAdapterPlaytest(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareComponentAdapterPlaytestValidator.BuildValidationReports();
            BattlePrepareComponentAdapterPlaytestController playtest =
                BattlePrepareComponentAdapterPlaytestValidator.BuildDefaultPlaytest();
            string[] reportPaths =
                BattlePrepareComponentAdapterPlaytestReportWriter.WriteReports(reports, playtest);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattlePrepareComponentAdapterPlaytest01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattlePrepareComponentAdapterPlaytest01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattlePrepareComponentAdapterRuntimePlaytest(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareComponentAdapterRuntimePlaytestValidator.BuildValidationReports();
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan =
                BattlePrepareComponentAdapterRuntimePlaytestValidator.BuildDefaultPlan();
            string[] reportPaths =
                BattlePrepareComponentAdapterRuntimePlaytestReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattlePrepareComponentAdapterRuntimePlaytest01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattlePrepareComponentAdapterRuntimePlaytest01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattlePrepareRuntimePlaytestFixtureFeedback(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareRuntimePlaytestFixtureFeedbackValidator.BuildValidationReports();
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan =
                BattlePrepareRuntimePlaytestFixtureFeedbackValidator.BuildDefaultPlan();
            string[] reportPaths =
                BattlePrepareRuntimePlaytestFixtureFeedbackReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattlePrepareRuntimePlaytestFixtureFeedbackFix01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattlePrepareRuntimePlaytestFixtureFeedbackFix01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunShapeAwareItemTrayFixture(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                ShapeAwareItemTrayValidator.BuildValidationReports();
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan =
                ShapeAwareItemTrayValidator.BuildDefaultPlan();
            string[] reportPaths =
                ShapeAwareItemTrayReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-ShapeAwareItemTrayFixtureFix02] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ShapeAwareItemTrayFixtureFix02 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunShapeAwareTrayPackingDrag(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                ShapeAwareTrayPackingValidator.BuildValidationReports();
            BattlePrepareComponentAdapterRuntimePlaytestPlan plan =
                ShapeAwareTrayPackingValidator.BuildDefaultPlan();
            string[] reportPaths =
                ShapeAwareTrayPackingReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-ShapeAwareTrayPackingDragFix03] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ShapeAwareTrayPackingDragFix03 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunShapePlacementSession(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                ShapePlacementSessionValidator.BuildValidationReports();
            ShapePlacementSessionValidationPlan plan =
                ShapePlacementSessionValidator.BuildDefaultPlan();
            string[] reportPaths =
                ShapePlacementSessionReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-ShapePlacementSession01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ShapePlacementSession01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunShapeAwareItemTrayGrid(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                ShapeAwareItemTrayGridValidator.BuildValidationReports();
            ShapeAwareItemTrayGridValidationPlan plan =
                ShapeAwareItemTrayGridValidator.BuildDefaultPlan();
            string[] reportPaths =
                ShapeAwareItemTrayGridReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-ShapeAwareItemTrayGrid01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ShapeAwareItemTrayGrid01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunMobileShapePlacement(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                MobileShapePlacementValidator.BuildValidationReports();
            MobileShapePlacementValidationPlan plan =
                MobileShapePlacementValidator.BuildDefaultPlan();
            string[] reportPaths =
                MobileShapePlacementReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-MobileShapePlacementInteraction01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox MobileShapePlacementInteraction01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunMobileShapePlacementRuntimeIntegration(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                MobileShapePlacementRuntimeIntegrationValidator.BuildValidationReports();
            MobileShapePlacementRuntimeIntegrationPlan plan =
                MobileShapePlacementRuntimeIntegrationValidator.BuildDefaultPlan();
            string[] reportPaths =
                MobileShapePlacementRuntimeIntegrationReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-MobileShapePlacementRuntimeIntegrationFix01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox MobileShapePlacementRuntimeIntegrationFix01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattlePrepareExtensionSeam(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattlePrepareExtensionSeamValidator.BuildValidationReports();
            BattlePrepareExtensionSeamPlan plan =
                BattlePrepareExtensionSeamValidator.BuildDefaultPlan();
            string[] reportPaths =
                BattlePrepareExtensionSeamReportWriter.WriteReports(reports, plan);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattlePrepareExtensionSeam01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattlePrepareExtensionSeam01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildTuningDataPanelPreview(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BuildTuningDataPanelPreviewValidator.BuildValidationReports();
            BuildTuningDataPanelPreview preview =
                BuildTuningDataPanelPreviewValidator.BuildDefaultPreview();
            string[] reportPaths =
                BuildTuningDataPanelPreviewReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BuildTuningDataPanelPreview01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BuildTuningDataPanelPreview01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunMechanicHintFeedbackPreview(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                MechanicHintFeedbackPreviewValidator.BuildValidationReports();
            MechanicHintFeedbackPreview preview =
                MechanicHintFeedbackPreviewValidator.BuildDefaultPreview();
            string[] reportPaths =
                MechanicHintFeedbackPreviewReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-MechanicHintFeedbackPreview01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox MechanicHintFeedbackPreview01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattleSandboxEnemyEncounterPreview(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattleSandboxEnemyEncounterPreviewValidator.BuildValidationReports();
            BattleSandboxEnemyEncounterPreview preview =
                BattleSandboxEnemyEncounterPreviewValidator.BuildDefaultPreview();
            string[] reportPaths =
                BattleSandboxEnemyEncounterPreviewReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattleSandboxEnemyEncounterPreview01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattleSandboxEnemyEncounterPreview01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattleSandboxEnemyCombatFeedbackUiReuse(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattleSandboxEnemyCombatFeedbackUiReuseValidator.BuildValidationReports();
            BattleSandboxEnemyCombatFeedbackPreview preview =
                BattleSandboxEnemyCombatFeedbackUiReuseValidator.BuildDefaultPreview();
            string[] reportPaths =
                BattleSandboxEnemyCombatFeedbackUiReuseReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattleSandboxEnemyCombatFeedbackUiReuse01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattleSandboxEnemyCombatFeedbackUiReuse01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattleSandboxBuildCombatPreview(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattleSandboxBuildCombatPreviewValidator.BuildValidationReports();
            BattleSandboxBuildCombatPreview preview =
                BattleSandboxBuildCombatPreviewValidator.BuildDefaultPreview();
            string[] reportPaths =
                BattleSandboxBuildCombatPreviewReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattleSandboxBuildCombatPreview01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattleSandboxBuildCombatPreview01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildSandboxItemStatFoundation(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxItemStatFoundationValidator.BuildValidationReports();
            BattleSandboxBuildCombatPreview preview =
                BuildSandboxItemStatFoundationValidator.BuildDefaultPreview();
            string[] reportPaths =
                BuildSandboxItemStatFoundationReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-ItemStatFoundation01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ItemStatFoundation01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildSandboxItemIdentityFamilyCorrection(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxItemIdentityFamilyCorrectionValidator.BuildValidationReports();
            BattleSandboxBuildCombatPreview preview =
                BuildSandboxItemIdentityFamilyCorrectionValidator.BuildDefaultPreview();
            string[] reportPaths =
                BuildSandboxItemIdentityFamilyCorrectionReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-ItemIdentityFamilyCorrection01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ItemIdentityFamilyCorrection01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildSandboxItemStatCombatPreview(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BuildSandboxItemStatCombatPreviewValidator.BuildValidationReports();
            BattleSandboxBuildCombatPreview preview =
                BuildSandboxItemStatCombatPreviewValidator.BuildDefaultPreview();
            string[] reportPaths =
                BuildSandboxItemStatCombatPreviewReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-ItemStatCombatPreview01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ItemStatCombatPreview01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattleSandboxManaLoopRuntime(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattleSandboxManaLoopRuntimeValidator.BuildValidationReports();
            BattleSandboxManaLoopPreview preview =
                BattleSandboxManaLoopRuntimeValidator.BuildDefaultPreview();
            string[] reportPaths =
                BattleSandboxManaLoopRuntimeReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattleSandboxManaLoopRuntime01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattleSandboxManaLoopRuntime01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunFormationCorePowerRange(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                FormationCorePowerRangeValidator.BuildValidationReports();
            BattleSandboxBuildCombatPreview combatPreview =
                FormationCorePowerRangeValidator.BuildDefaultPreview();
            FormationCorePowerRangePreview powerPreview =
                FormationCorePowerRangeResolver.Apply(combatPreview?.context?.layoutSnapshot);
            string[] reportPaths =
                FormationCorePowerRangeReportWriter.WriteReports(reports, powerPreview, combatPreview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-FormationCoreAndPowerRange01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox FormationCoreAndPowerRange01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattleSandboxRuntimeLoop(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattleSandboxRuntimeLoopValidator.BuildValidationReports();
            BattleSandboxRuntimeLoopPreview preview =
                BattleSandboxRuntimeLoopValidator.BuildDefaultPreview();
            string[] reportPaths =
                BattleSandboxRuntimeLoopReportWriter.WriteReports(reports, preview);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattleSandboxRuntimeLoop01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattleSandboxRuntimeLoop01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBattleSandboxItemEffectRuntimePreview(bool throwOnFailure)
        {
            return BattleSandboxItemEffectRuntimePreviewValidator.Run(throwOnFailure);
        }

        public static bool RunBattleSandboxPlayableLoop(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                BattleSandboxPlayableLoopValidator.BuildValidationReports();
            BattleSandboxPlayableLoopSnapshot snapshot =
                BattleSandboxPlayableLoopValidator.BuildDefaultSnapshot();
            string[] reportPaths =
                BattleSandboxPlayableLoopReportWriter.WriteReports(reports, snapshot);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                switch (issue.Level)
                {
                    case BuildSandboxValidationLevel.Error:
                        Debug.LogError(issue.ToString());
                        break;
                    case BuildSandboxValidationLevel.Warning:
                        Debug.LogWarning(issue.ToString());
                        break;
                    default:
                        Debug.Log(issue.ToString());
                        break;
                }
            }

            Debug.Log(
                $"[BuildSandbox-BattleSandboxPlayableLoop01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BattleSandboxPlayableLoop01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildSandboxPlayableRegression(bool throwOnFailure)
        {
            return BuildSandboxPlayableRegressionValidator.Run(throwOnFailure);
        }

        public static bool RunBattleSandboxPlayableFullRosterRegression(bool throwOnFailure)
        {
            return BattleSandboxPlayableFullRosterRegressionValidator.Run(throwOnFailure);
        }

        public static bool RunBattleSandboxDevChapterPlayable(bool throwOnFailure)
        {
            return BattleSandboxDevChapterPlayableValidator.Run(throwOnFailure);
        }
    }
}
#endif
