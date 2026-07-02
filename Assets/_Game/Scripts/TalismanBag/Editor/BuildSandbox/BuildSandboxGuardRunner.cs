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
        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/GuardBaseline01/[QA Only] Run Guard Baseline")]
        public static void RunGuardBaselineMenu()
        {
            RunGuardBaseline(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/SynergyDataFoundation01/[QA Only] Run Data Foundation")]
        public static void RunSynergyDataFoundationMenu()
        {
            RunSynergyDataFoundation(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/ItemShapeOccupancy01/[QA Only] Run Shape Occupancy")]
        public static void RunItemShapeOccupancyMenu()
        {
            RunItemShapeOccupancy(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/SynergyEvaluatorCore01/[QA Only] Run Synergy Evaluator Core")]
        public static void RunSynergyEvaluatorCoreMenu()
        {
            RunSynergyEvaluatorCore(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/AffixRaritySandbox01/[QA Only] Run Affix Rarity Sandbox")]
        public static void RunAffixRaritySandboxMenu()
        {
            RunAffixRaritySandbox(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/ModifierEventBridge01/[QA Only] Run Modifier Event Bridge")]
        public static void RunModifierEventBridgeMenu()
        {
            RunModifierEventBridge(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildSimulationBenchmark01/[QA Only] Run Build Simulation Benchmark")]
        public static void RunBuildSimulationBenchmarkMenu()
        {
            RunBuildSimulationBenchmark(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/EnemyBossValidationPool01/[QA Only] Run Enemy Boss Validation Pool")]
        public static void RunEnemyBossValidationPoolMenu()
        {
            RunEnemyBossValidationPool(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/DevChapterContentPool01/[QA Only] Run Dev Chapter Content Pool")]
        public static void RunDevChapterContentPoolMenu()
        {
            RunDevChapterContentPool(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/LedgerTaskBuildHooks01/[QA Only] Run Ledger Task Build Hooks")]
        public static void RunLedgerTaskBuildHooksMenu()
        {
            RunLedgerTaskBuildHooks(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/ConfigValidatorReport01/[QA Only] Run Config Validator Report")]
        public static void RunConfigValidatorReportMenu()
        {
            RunConfigValidatorReport(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/FinalIntegrationDryRun01/[QA Only] Run Final Integration Dry Run")]
        public static void RunFinalIntegrationDryRunMenu()
        {
            RunFinalIntegrationDryRun(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildProblemRulePool01/[QA Only] Run Build Problem Rule Pool")]
        public static void RunBuildProblemRulePoolMenu()
        {
            RunBuildProblemRulePool(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildProblemSeedData01/[QA Only] Run Build Problem Seed Data")]
        public static void RunBuildProblemSeedDataMenu()
        {
            RunBuildProblemSeedData(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildSandboxConfigPanel01/[QA Only] Run BuildSandbox Config Panel")]
        public static void RunBuildSandboxConfigPanelMenu()
        {
            RunBuildSandboxConfigPanel(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxPreviewScene01/[QA Only] Verify Preview Scene")]
        public static void RunBattleSandboxPreviewSceneMenu()
        {
            RunBattleSandboxPreviewScene(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildSandboxPreviewContext01/[QA Only] Run Preview Context")]
        public static void RunBuildSandboxPreviewContextMenu()
        {
            RunBuildSandboxPreviewContext(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BattlePageViewAdapter01/[QA Only] Run Battle Page View Adapter")]
        public static void RunBattlePageViewAdapterMenu()
        {
            RunBattlePageViewAdapter(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildGridInteractionPreview01/[QA Only] Run Grid Interaction Preview")]
        public static void RunBuildGridInteractionPreviewMenu()
        {
            RunBuildGridInteractionPreview(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapter01/[QA Only] Run BattlePrepare Component Adapter")]
        public static void RunBattlePrepareComponentAdapterMenu()
        {
            RunBattlePrepareComponentAdapter(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BattlePrepareComponentAdapterPlaytest01/[QA Only] Run Battle Prepare Component Adapter Playtest")]
        public static void RunBattlePrepareComponentAdapterPlaytestMenu()
        {
            RunBattlePrepareComponentAdapterPlaytest(throwOnFailure: false);
        }

        [MenuItem(BattlePrepareComponentAdapterRuntimePlaytestPlan.ManualMenuPath)]
        public static void OpenBattlePrepareComponentAdapterRuntimePlaytestMenu()
        {
            BattlePrepareComponentAdapterRuntimePlaytestLauncher.OpenRuntimePlaytest();
        }

        [MenuItem(BattlePrepareComponentAdapterRuntimePlaytestPlan.QaMenuPath)]
        public static void RunBattlePrepareComponentAdapterRuntimePlaytestMenu()
        {
            RunBattlePrepareComponentAdapterRuntimePlaytest(throwOnFailure: false);
        }

        [MenuItem(BattlePrepareComponentAdapterRuntimePlaytestPlan.FixtureFeedbackQaMenuPath)]
        public static void RunBattlePrepareRuntimePlaytestFixtureFeedbackMenu()
        {
            RunBattlePrepareRuntimePlaytestFixtureFeedback(throwOnFailure: false);
        }

        [MenuItem(BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareItemTrayQaMenuPath)]
        public static void RunShapeAwareItemTrayFixtureMenu()
        {
            RunShapeAwareItemTrayFixture(throwOnFailure: false);
        }

        [MenuItem(BattlePrepareComponentAdapterRuntimePlaytestPlan.ShapeAwareTrayPackingDragQaMenuPath)]
        public static void RunShapeAwareTrayPackingDragMenu()
        {
            RunShapeAwareTrayPackingDrag(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/ShapePlacementSession01/[QA Only] Run Shape Placement Session")]
        public static void RunShapePlacementSessionMenu()
        {
            RunShapePlacementSession(throwOnFailure: false);
        }

        [MenuItem(ShapeAwareItemTrayGridValidator.QaMenuPath)]
        public static void RunShapeAwareItemTrayGridMenu()
        {
            RunShapeAwareItemTrayGrid(throwOnFailure: false);
        }

        [MenuItem(MobileShapePlacementValidator.QaMenuPath)]
        public static void RunMobileShapePlacementMenu()
        {
            RunMobileShapePlacement(throwOnFailure: false);
        }

        [MenuItem(MobileShapePlacementRuntimeIntegrationValidator.QaMenuPath)]
        public static void RunMobileShapePlacementRuntimeIntegrationMenu()
        {
            RunMobileShapePlacementRuntimeIntegration(throwOnFailure: false);
        }

        [MenuItem(BattlePrepareExtensionSeamValidator.QaMenuPath)]
        public static void RunBattlePrepareExtensionSeamMenu()
        {
            RunBattlePrepareExtensionSeam(throwOnFailure: false);
        }

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BuildTuningDataPanelPreview01/[QA Only] Run Build Tuning Data Panel Preview")]
        public static void RunBuildTuningDataPanelPreviewMenu()
        {
            RunBuildTuningDataPanelPreview(throwOnFailure: false);
        }

        [MenuItem(MechanicHintFeedbackPreviewValidator.QaMenuPath)]
        public static void RunMechanicHintFeedbackPreviewMenu()
        {
            RunMechanicHintFeedbackPreview(throwOnFailure: false);
        }

        public static void RunGuardBaselineBatch()
        {
            bool passed = RunGuardBaseline(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunSynergyDataFoundationBatch()
        {
            bool passed = RunSynergyDataFoundation(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunItemShapeOccupancyBatch()
        {
            bool passed = RunItemShapeOccupancy(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunSynergyEvaluatorCoreBatch()
        {
            bool passed = RunSynergyEvaluatorCore(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunAffixRaritySandboxBatch()
        {
            bool passed = RunAffixRaritySandbox(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunModifierEventBridgeBatch()
        {
            bool passed = RunModifierEventBridge(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildSimulationBenchmarkBatch()
        {
            bool passed = RunBuildSimulationBenchmark(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunEnemyBossValidationPoolBatch()
        {
            bool passed = RunEnemyBossValidationPool(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunDevChapterContentPoolBatch()
        {
            bool passed = RunDevChapterContentPool(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunLedgerTaskBuildHooksBatch()
        {
            bool passed = RunLedgerTaskBuildHooks(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunConfigValidatorReportBatch()
        {
            bool passed = RunConfigValidatorReport(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunFinalIntegrationDryRunBatch()
        {
            bool passed = RunFinalIntegrationDryRun(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildProblemRulePoolBatch()
        {
            bool passed = RunBuildProblemRulePool(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        public static void RunBuildProblemSeedDataBatch()
        {
            bool passed = RunBuildProblemSeedData(throwOnFailure: true);
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
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

        public static bool RunGuardBaseline(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder()
            };

            string reportPath = BuildSandboxReportWriter.WriteGuardBaselineReport(reports);
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
                $"[BuildSandboxGuardBaseline01] completed errors={errors}, warnings={warnings}, report={reportPath}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox GuardBaseline failed with {errors} error(s). See {reportPath}");
            }

            return errors == 0;
        }

        public static bool RunSynergyDataFoundation(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder()
            };

            string reportPath = BuildSandboxReportWriter.WriteSynergyDataFoundationReport(reports);
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
                $"[BuildSandbox-SynergyDataFoundation01] completed errors={errors}, warnings={warnings}, report={reportPath}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox SynergyDataFoundation01 failed with {errors} error(s). See {reportPath}");
            }

            return errors == 0;
        }

        public static bool RunItemShapeOccupancy(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate()
            };

            string[] reportPaths = BuildSandboxReportWriter.WriteItemShapeOccupancyReports(reports);
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
                $"[BuildSandbox-ItemShapeOccupancy01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ItemShapeOccupancy01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunSynergyEvaluatorCore(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate()
            };

            string[] reportPaths = BuildSandboxReportWriter.WriteSynergyEvaluatorCoreReports(reports);
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
                $"[BuildSandbox-SynergyEvaluatorCore01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox SynergyEvaluatorCore01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunAffixRaritySandbox(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate()
            };

            string[] reportPaths = BuildSandboxReportWriter.WriteAffixRaritySandboxReports(reports);
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
                $"[BuildSandbox-AffixRaritySandbox01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox AffixRaritySandbox01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunModifierEventBridge(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate(),
                ModifierEventBridgeValidator.Validate()
            };

            string[] reportPaths = BuildSandboxReportWriter.WriteModifierEventBridgeReports(reports);
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
                $"[BuildSandbox-ModifierEventBridge01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ModifierEventBridge01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildSimulationBenchmark(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate(),
                ModifierEventBridgeValidator.Validate(),
                BuildSimulationBenchmarkValidator.Validate()
            };

            string[] reportPaths = BuildSimulationBenchmarkReportWriter.WriteReports(reports);
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
                $"[BuildSandbox-BuildSimulationBenchmark01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BuildSimulationBenchmark01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunEnemyBossValidationPool(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate(),
                ModifierEventBridgeValidator.Validate(),
                BuildSimulationBenchmarkValidator.Validate(),
                EnemyBossValidationPoolValidator.Validate()
            };

            string[] reportPaths = EnemyBossValidationPoolReportWriter.WriteReports(reports);
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
                $"[BuildSandbox-EnemyBossValidationPool01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox EnemyBossValidationPool01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunDevChapterContentPool(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate(),
                ModifierEventBridgeValidator.Validate(),
                BuildSimulationBenchmarkValidator.Validate(),
                EnemyBossValidationPoolValidator.Validate(),
                DevChapterContentPoolValidator.Validate()
            };

            string[] reportPaths = DevChapterContentPoolReportWriter.WriteReports(reports);
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
                $"[BuildSandbox-DevChapterContentPool01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox DevChapterContentPool01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunLedgerTaskBuildHooks(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate(),
                ModifierEventBridgeValidator.Validate(),
                BuildSimulationBenchmarkValidator.Validate(),
                EnemyBossValidationPoolValidator.Validate(),
                DevChapterContentPoolValidator.Validate(),
                LedgerTaskBuildHooksValidator.Validate()
            };

            string[] reportPaths = LedgerTaskBuildHooksReportWriter.WriteReports(reports);
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
                $"[BuildSandbox-LedgerTaskBuildHooks01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox LedgerTaskBuildHooks01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunConfigValidatorReport(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                ItemShapeOccupancyValidator.Validate(),
                SynergyEvaluatorCoreValidator.Validate(),
                AffixRaritySandboxValidator.Validate(),
                ModifierEventBridgeValidator.Validate(),
                BuildSimulationBenchmarkValidator.Validate(),
                EnemyBossValidationPoolValidator.Validate(),
                DevChapterContentPoolValidator.Validate(),
                LedgerTaskBuildHooksValidator.Validate()
            };

            List<string> reportPaths = new()
            {
                BuildSandboxReportWriter.WriteGuardBaselineReport(reports),
                BuildSandboxReportWriter.WriteSynergyDataFoundationReport(reports)
            };
            reportPaths.AddRange(BuildSandboxReportWriter.WriteItemShapeOccupancyReports(reports));
            reportPaths.AddRange(BuildSandboxReportWriter.WriteSynergyEvaluatorCoreReports(reports));
            reportPaths.AddRange(BuildSandboxReportWriter.WriteAffixRaritySandboxReports(reports));
            reportPaths.AddRange(BuildSandboxReportWriter.WriteModifierEventBridgeReports(reports));
            reportPaths.AddRange(BuildSimulationBenchmarkReportWriter.WriteReports(reports));
            reportPaths.AddRange(EnemyBossValidationPoolReportWriter.WriteReports(reports));
            reportPaths.AddRange(DevChapterContentPoolReportWriter.WriteReports(reports));
            reportPaths.AddRange(LedgerTaskBuildHooksReportWriter.WriteReports(reports));
            reportPaths.AddRange(ConfigValidatorReportWriter.WriteReports(reports, reportPaths));

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
                $"[BuildSandbox-ConfigValidatorReport01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox ConfigValidatorReport01 failed with {errors} error(s). See {ConfigValidatorReportWriter.ConfigValidationReportPath}");
            }

            return errors == 0;
        }

        public static bool RunFinalIntegrationDryRun(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports =
                FinalIntegrationDryRunReportWriter.BuildValidationReports();

            List<string> reportPaths = new()
            {
                BuildSandboxReportWriter.WriteGuardBaselineReport(reports),
                BuildSandboxReportWriter.WriteSynergyDataFoundationReport(reports)
            };
            reportPaths.AddRange(BuildSandboxReportWriter.WriteItemShapeOccupancyReports(reports));
            reportPaths.AddRange(BuildSandboxReportWriter.WriteSynergyEvaluatorCoreReports(reports));
            reportPaths.AddRange(BuildSandboxReportWriter.WriteAffixRaritySandboxReports(reports));
            reportPaths.AddRange(BuildSandboxReportWriter.WriteModifierEventBridgeReports(reports));
            reportPaths.AddRange(BuildSimulationBenchmarkReportWriter.WriteReports(reports));
            reportPaths.AddRange(EnemyBossValidationPoolReportWriter.WriteReports(reports));
            reportPaths.AddRange(DevChapterContentPoolReportWriter.WriteReports(reports));
            reportPaths.AddRange(LedgerTaskBuildHooksReportWriter.WriteReports(reports));
            reportPaths.AddRange(ConfigValidatorReportWriter.WriteReports(reports, reportPaths));
            reportPaths.AddRange(FinalIntegrationDryRunReportWriter.WriteReports(reports, reportPaths));

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
                $"[BuildSandbox-FinalIntegrationDryRun01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox FinalIntegrationDryRun01 failed with {errors} error(s). See {FinalIntegrationDryRunReportWriter.FinalIntegrationDryRunReportPath}");
            }

            return errors == 0;
        }

        public static bool RunBuildProblemRulePool(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                BuildProblemRulePoolValidator.Validate()
            };

            string[] reportPaths = BuildProblemRulePoolReportWriter.WriteReports(reports);
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
                $"[BuildSandbox-BuildProblemRulePool01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BuildProblemRulePool01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static bool RunBuildProblemSeedData(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = new()
            {
                BuildSandboxConfigValidator.Validate(),
                BuildSandboxDevOnlyValidator.Validate(),
                BuildSandboxUiLayoutGuard.Validate(),
                BuildSandboxCoreFlowSmokeEntry.ValidatePlaceholder(),
                BuildProblemRulePoolValidator.Validate(),
                BuildProblemSeedDataValidator.Validate()
            };

            string[] reportPaths = BuildProblemSeedDataReportWriter.WriteReports(reports);
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
                $"[BuildSandbox-BuildProblemSeedData01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox BuildProblemSeedData01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
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
    }
}
#endif
