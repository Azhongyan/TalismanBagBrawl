#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class FinalIntegrationDryRunReportWriter
    {
        public const string FinalIntegrationDryRunReportPath =
            "Docs/V0.4/Reports/FinalIntegrationDryRunReport.md";

        public const string FullPipelineReportPath =
            "Docs/V0.4/Reports/BuildSandboxFullPipelineReport.md";

        public const string FeatureFlagFinalCheckPath =
            "Docs/V0.4/Reports/BuildSandboxFeatureFlagFinalCheck.md";

        public const string DevOnlyFinalCheckPath =
            "Docs/V0.4/Reports/BuildSandboxDevOnlyFinalCheck.md";

        public const string FormalFlowLeakFinalCheckPath =
            "Docs/V0.4/Reports/BuildSandboxFormalFlowLeakFinalCheck.md";

        private static readonly string[] PreviousPackages =
        {
            "GuardBaseline01",
            "SynergyDataFoundation01",
            "ItemShapeOccupancy01",
            "SynergyEvaluatorCore01",
            "AffixRaritySandbox01",
            "ModifierEventBridge01",
            "BuildSimulationBenchmark01",
            "EnemyBossValidationPool01",
            "DevChapterContentPool01",
            "LedgerTaskBuildHooks01",
            "ConfigValidatorReport01"
        };

        private static readonly string[] RuntimeSourceRoots =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox"
        };

        private static readonly string[] ForbiddenRuntimeTokens =
        {
            "StageConfig",
            "RewardConfig",
            "RewardDropTable",
            "UpgradeConfig",
            "SaveData",
            "PlayerPrefs",
            "MainTrialProgressData",
            "RunFlow",
            "PageState",
            "FormationState",
            "V02RunFlowController",
            "V02FormationGridFrame",
            "DamageText",
            "BossReward",
            "FormalDrop",
            "FormalForge"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            FinalIntegrationDryRunContext context = BuildContext();
            return new List<BuildSandboxValidationReport>
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
                LedgerTaskBuildHooksValidator.Validate(),
                ValidateFinalIntegration(context)
            };
        }

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            IReadOnlyList<string> generatedReportPaths)
        {
            FinalIntegrationDryRunContext context = BuildContext();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string finalPath = Path.Combine(projectRoot, FinalIntegrationDryRunReportPath);
            string pipelinePath = Path.Combine(projectRoot, FullPipelineReportPath);
            string featureFlagPath = Path.Combine(projectRoot, FeatureFlagFinalCheckPath);
            string devOnlyPath = Path.Combine(projectRoot, DevOnlyFinalCheckPath);
            string leakPath = Path.Combine(projectRoot, FormalFlowLeakFinalCheckPath);
            Directory.CreateDirectory(Path.GetDirectoryName(finalPath) ?? projectRoot);

            List<string> reportIndex = MergeReportIndex(generatedReportPaths);
            reportIndex.AddRange(new[]
            {
                FinalIntegrationDryRunReportPath,
                FullPipelineReportPath,
                FeatureFlagFinalCheckPath,
                DevOnlyFinalCheckPath,
                FormalFlowLeakFinalCheckPath
            });
            reportIndex = reportIndex
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();

            File.WriteAllText(
                finalPath,
                BuildFinalIntegrationMarkdown(context, reports, reportIndex),
                new UTF8Encoding(false));
            File.WriteAllText(
                pipelinePath,
                BuildFullPipelineMarkdown(context, reports),
                new UTF8Encoding(false));
            File.WriteAllText(
                featureFlagPath,
                BuildFeatureFlagMarkdown(context, reports),
                new UTF8Encoding(false));
            File.WriteAllText(
                devOnlyPath,
                BuildDevOnlyMarkdown(context, reports),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildFormalLeakMarkdown(context, reports),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { finalPath, pipelinePath, featureFlagPath, devOnlyPath, leakPath };
        }

        private static BuildSandboxValidationReport ValidateFinalIntegration(FinalIntegrationDryRunContext context)
        {
            BuildSandboxValidationReport report = new("Final Integration Dry Run");

            if (context.FeatureFlagDefaultTrue > 0)
            {
                report.AddError(
                    "FINAL_FEATURE_FLAG_DEFAULT_TRUE",
                    $"All BuildSandbox FeatureFlags must default false; defaultTrue={context.FeatureFlagDefaultTrue}.",
                    nameof(BuildSandboxFeatureFlags));
            }
            else
            {
                report.AddInfo(
                    "FINAL_FEATURE_FLAGS_ALL_FALSE",
                    "All BuildSandbox FeatureFlags default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if (context.NonDevOnlyConfigCount > 0 || context.EnabledConfigCount > 0)
            {
                report.AddError(
                    "FINAL_DEVONLY_ISOLATION_FAIL",
                    $"BuildSandbox config isolation failed. devOnly=false={context.NonDevOnlyConfigCount}, isEnabled=true={context.EnabledConfigCount}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "FINAL_DEVONLY_ISOLATION_PASS",
                    $"BuildSandbox configs isolated. scanned={context.Configs.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (context.ShapeSnapshot.placedItems.Count == 0
                || context.ShapeSnapshot.placedItems.Any(item => item.occupiedCells == null || item.occupiedCells.Count == 0))
            {
                report.AddError(
                    "FINAL_OCCUPIED_CELLS_MISSING",
                    "ItemShapeOccupancy must output occupiedCells for valid placed items.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "FINAL_OCCUPIED_CELLS_PRESENT",
                    $"ItemShapeOccupancy output placedItems={context.ShapeSnapshot.placedItems.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (context.BuildEvaluation == null || !context.BuildEvaluation.HasActiveSynergy)
            {
                report.AddError(
                    "FINAL_SYNERGY_EVALUATION_EMPTY",
                    "SynergyEvaluator must activate at least one sandbox synergy.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "FINAL_SYNERGY_EVALUATION_PASS",
                    $"activeSynergies={context.BuildEvaluation.activeSynergies.Count}, activeThresholds={context.BuildEvaluation.activeThresholds.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (context.AffixEvaluation == null
                || context.AffixEvaluation.itemResults.Count == 0
                || context.AffixEvaluation.affixIds.Count == 0)
            {
                report.AddError(
                    "FINAL_AFFIX_PREVIEW_EMPTY",
                    "AffixRaritySandbox must generate item and affix preview output.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "FINAL_AFFIX_PREVIEW_PASS",
                    $"affixItems={context.AffixEvaluation.itemResults.Count}, affixes={context.AffixEvaluation.affixIds.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (context.ModifierBundle == null
                || context.EventBundle == null
                || context.ModifierBundle.modifiers.Count == 0
                || context.EventBundle.events.Count == 0)
            {
                report.AddError(
                    "FINAL_MODIFIER_EVENT_EMPTY",
                    "ModifierBundle and EventBundle previews must be generated.",
                    nameof(ModifierEventBridge));
            }
            else if (CountModifierLeaks(context.ModifierBundle, context.EventBundle) > 0)
            {
                report.AddError(
                    "FINAL_MODIFIER_EVENT_LEAK",
                    "ModifierBundle / EventBundle preview output must stay devOnly, disabled, and formal-combat isolated.",
                    nameof(ModifierEventBridge));
            }
            else
            {
                report.AddInfo(
                    "FINAL_MODIFIER_EVENT_PASS",
                    $"modifiers={context.ModifierBundle.modifiers.Count}, events={context.EventBundle.events.Count}.",
                    nameof(ModifierEventBridge));
            }

            if (context.Benchmark.results.Count == 0
                || context.EnemyBossBenchmark.results.Count == 0
                || context.DevChapterBenchmark.results.Count == 0)
            {
                report.AddError(
                    "FINAL_BENCHMARK_EMPTY",
                    "Benchmark, Enemy/Boss pool, and DevChapter pool simulations must all produce results.",
                    nameof(BuildSimulationRunner));
            }
            else
            {
                report.AddInfo(
                    "FINAL_BENCHMARK_PASS",
                    $"benchmark={context.Benchmark.results.Count}, enemyBoss={context.EnemyBossBenchmark.results.Count}, devChapter={context.DevChapterBenchmark.results.Count}.",
                    nameof(BuildSimulationRunner));
            }

            if (context.LedgerHook == null
                || context.LedgerHook.taskGoals.Count == 0
                || context.LedgerEvents.Count == 0
                || context.LedgerProgress.Count == 0)
            {
                report.AddError(
                    "FINAL_LEDGER_HOOK_EMPTY",
                    "Ledger build task hook preview must output goals, events, and progress.",
                    nameof(LedgerBuildTaskHook));
            }
            else
            {
                report.AddInfo(
                    "FINAL_LEDGER_HOOK_PASS",
                    $"goals={context.LedgerHook.taskGoals.Count}, events={context.LedgerEvents.Count}, progress={context.LedgerProgress.Count}.",
                    nameof(LedgerBuildTaskHook));
            }

            if (context.Leaks.Total > 0)
            {
                report.AddError(
                    "FINAL_FORMAL_FLOW_LEAK",
                    $"Formal flow leak counters must all be zero; total={context.Leaks.Total}.",
                    "BuildSandbox Final Leak Check");
            }
            else
            {
                report.AddInfo(
                    "FINAL_FORMAL_FLOW_LEAK_PASS",
                    "Formal flow leak counters are all zero.",
                    "BuildSandbox Final Leak Check");
            }

            if (context.RuntimeSourceForbiddenTokenHits.Count > 0)
            {
                report.AddError(
                    "FINAL_RUNTIME_SOURCE_FORMAL_TOKEN",
                    $"Runtime BuildSandbox source contains forbidden formal token hit(s): {context.RuntimeSourceForbiddenTokenHits.Count}.",
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox");
            }
            else
            {
                report.AddInfo(
                    "FINAL_RUNTIME_SOURCE_SCAN_PASS",
                    "Runtime BuildSandbox source scan found no forbidden formal-system tokens outside comments and strings.",
                    "Assets/_Game/Scripts/TalismanBag/BuildSandbox");
            }

            return report;
        }

        private static FinalIntegrationDryRunContext BuildContext()
        {
            BuildSandboxLayoutSnapshot shapeSnapshot = ItemShapeOccupancyValidator.BuildSampleSnapshot();
            BuildEvaluationResult buildEvaluation = SynergyEvaluatorCoreValidator.BuildSampleEvaluation(out BuildSandboxLayoutSnapshot synergySnapshot);
            AffixRarityEvaluationResult affixEvaluation = AffixRaritySandboxValidator.BuildSampleEvaluation(out BuildSandboxLayoutSnapshot affixSnapshot);
            ModifierEventBridgeValidator.BuildSamplePreview(
                out _,
                out _,
                out CombatModifierBundle modifierBundle,
                out EffectEventBundle eventBundle);
            BuildSimulationBenchmarkReport benchmark = BuildSimulationBenchmarkValidator.BuildSampleBenchmark();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            BuildSimulationBenchmarkReport enemyBossBenchmark = EnemyBossValidationPoolValidator.BuildPoolSimulationBenchmark();
            DevChapterContentPool devChapterPool = DevChapterContentPool.CreateDefault();
            BuildSimulationBenchmarkReport devChapterBenchmark = DevChapterContentPoolValidator.BuildDevChapterSimulationBenchmark();
            LedgerTaskBuildHooksValidator.BuildPreview(
                out LedgerBuildTaskHook ledgerHook,
                out List<LedgerBuildTaskEvent> ledgerEvents,
                out List<LedgerBuildTaskProgressPreview> ledgerProgress);

            FinalIntegrationDryRunContext context = new()
            {
                Configs = BuildSandboxConfigValidator.CollectConfigAssets().ToList(),
                ItemTags = BuildSandboxConfigValidator.CollectItemTagConfigs().ToList(),
                Synergies = BuildSandboxConfigValidator.CollectSynergyConfigs().ToList(),
                Shapes = BuildSandboxConfigValidator.CollectItemShapeConfigs().ToList(),
                Rarities = BuildSandboxConfigValidator.CollectRarityTierConfigs().ToList(),
                Affixes = BuildSandboxConfigValidator.CollectAffixConfigs().ToList(),
                ShapeSnapshot = shapeSnapshot,
                SynergySnapshot = synergySnapshot,
                AffixSnapshot = affixSnapshot,
                BuildEvaluation = buildEvaluation,
                AffixEvaluation = affixEvaluation,
                ModifierBundle = modifierBundle,
                EventBundle = eventBundle,
                Benchmark = benchmark,
                EnemyBossPool = enemyBossPool,
                EnemyBossBenchmark = enemyBossBenchmark,
                DevChapterPool = devChapterPool,
                DevChapterBenchmark = devChapterBenchmark,
                LedgerHook = ledgerHook,
                LedgerEvents = ledgerEvents ?? new List<LedgerBuildTaskEvent>(),
                LedgerProgress = ledgerProgress ?? new List<LedgerBuildTaskProgressPreview>()
            };
            context.FeatureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            context.NonDevOnlyConfigCount = context.Configs.Count(config => !config.devOnly);
            context.EnabledConfigCount = context.Configs.Count(config => config.isEnabled);
            context.RuntimeSourceForbiddenTokenHits = ScanRuntimeSourceForbiddenTokens();
            context.Leaks = CountLeaks(context);
            return context;
        }

        private static string BuildFinalIntegrationMarkdown(
            FinalIntegrationDryRunContext context,
            IReadOnlyList<BuildSandboxValidationReport> reports,
            IReadOnlyList<string> reportIndex)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            StringBuilder builder = new();
            builder.AppendLine("# Final Integration Dry Run Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Total Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- BuildSandbox full-chain dry run only.");
            builder.AppendLine("- No FeatureFlag is opened.");
            builder.AppendLine("- No formal gameplay, drop, forge, combat, UI, save, reward, numeric, or Boss flow is connected.");
            builder.AppendLine("- Reports are generated from Editor-only validators and sandbox preview data.");
            builder.AppendLine();
            builder.AppendLine("## Previous Package Status");
            builder.AppendLine();
            builder.AppendLine("| Package | Status |");
            builder.AppendLine("| --- | --- |");
            foreach (string package in PreviousPackages)
            {
                builder.AppendLine($"| `{package}` | `USER_ACCEPTED / QA_PASSED_BY_USER / AVAILABLE_FOR_DRY_RUN` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Full-Chain Input");
            builder.AppendLine();
            builder.AppendLine($"- Config assets scanned: `{context.Configs.Count}`");
            builder.AppendLine($"- Item tags: `{context.ItemTags.Count}`");
            builder.AppendLine($"- Synergy configs: `{context.Synergies.Count}`");
            builder.AppendLine($"- Item shape configs: `{context.Shapes.Count}`");
            builder.AppendLine($"- Rarity tiers: `{context.Rarities.Count}`");
            builder.AppendLine($"- Affix configs: `{context.Affixes.Count}`");
            builder.AppendLine($"- Shape snapshot placed items: `{context.ShapeSnapshot.placedItems.Count}`");
            builder.AppendLine($"- Synergy snapshot placed items: `{context.SynergySnapshot.placedItems.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Full-Chain Output");
            builder.AppendLine();
            builder.AppendLine($"- Active synergies: `{context.BuildEvaluation.activeSynergies.Count}`");
            builder.AppendLine($"- Active thresholds: `{context.BuildEvaluation.activeThresholds.Count}`");
            builder.AppendLine($"- Affix preview items: `{context.AffixEvaluation.itemResults.Count}`");
            builder.AppendLine($"- Modifier previews: `{context.ModifierBundle.modifiers.Count}`");
            builder.AppendLine($"- Event previews: `{context.EventBundle.events.Count}`");
            builder.AppendLine($"- Benchmark results: `{context.Benchmark.results.Count}`");
            builder.AppendLine($"- Enemy/Boss validation results: `{context.EnemyBossBenchmark.results.Count}`");
            builder.AppendLine($"- DevChapter validation results: `{context.DevChapterBenchmark.results.Count}`");
            builder.AppendLine($"- Ledger goals / events / progress: `{context.LedgerHook.taskGoals.Count}` / `{context.LedgerEvents.Count}` / `{context.LedgerProgress.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Final Checks");
            builder.AppendLine();
            builder.AppendLine($"- FeatureFlag default true count: `{context.FeatureFlagDefaultTrue}`");
            builder.AppendLine($"- devOnly=false config count: `{context.NonDevOnlyConfigCount}`");
            builder.AppendLine($"- isEnabled=true config count: `{context.EnabledConfigCount}`");
            builder.AppendLine($"- Formal flow leak total: `{context.Leaks.Total}`");
            builder.AppendLine($"- Runtime source forbidden token hits: `{context.RuntimeSourceForbiddenTokenHits.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Validation Summary");
            AppendValidationSummary(builder, reports);
            builder.AppendLine();
            builder.AppendLine("## Report Index");
            builder.AppendLine();
            builder.AppendLine("| Report |");
            builder.AppendLine("| --- |");
            foreach (string reportPath in reportIndex)
            {
                builder.AppendLine($"| `{Escape(reportPath)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Phase Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "`V0.3/V0.4-BuildSandbox Technical Queue` can be marked `PHASE_COMPLETE / WAITING_REPOOPS_CHECKPOINT` after user confirmation. Future formal hookup still requires a new Roadmap / Package Queue."
                : "Do not mark the BuildSandbox queue phase complete until FinalIntegrationDryRun01 errors are resolved under an approved boundary.");
            return builder.ToString();
        }

        private static string BuildFullPipelineMarkdown(
            FinalIntegrationDryRunContext context,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            int errors = reports.Sum(report => report.ErrorCount);

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Full Pipeline Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Pipeline Stages");
            builder.AppendLine();
            builder.AppendLine("| Stage | Input | Output | Isolation |");
            builder.AppendLine("| --- | --- | --- | --- |");
            builder.AppendLine($"| `BuildSandboxLayoutSnapshot` | `{context.ShapeSnapshot.placedItems.Count}` shape placed item(s) | occupiedCells populated | devOnly preview only |");
            builder.AppendLine($"| `SynergyEvaluator` | `{context.SynergySnapshot.placedItems.Count}` synergy placed item(s) | `{context.BuildEvaluation.activeSynergies.Count}` active synergy result(s) | no formal combat hookup |");
            builder.AppendLine($"| `AffixRarityEvaluator` | `{context.AffixSnapshot.placedItems.Count}` affix placed item(s) | `{context.AffixEvaluation.itemResults.Count}` item preview result(s) | no drop / wash / reward hookup |");
            builder.AppendLine($"| `ModifierEventBridge` | build + affix preview | `{context.ModifierBundle.modifiers.Count}` modifier(s), `{context.EventBundle.events.Count}` event(s) | affectsFormalCombat=false |");
            builder.AppendLine($"| `BuildSimulationBenchmark` | sandbox preview bundles | `{context.Benchmark.results.Count}` benchmark result(s) | sandboxEstimate=true |");
            builder.AppendLine($"| `EnemyBossValidationPool` | `{context.EnemyBossPool.enemies.Count}` enemy + `{context.EnemyBossPool.bosses.Count}` boss profile(s) | `{context.EnemyBossBenchmark.results.Count}` simulation result(s) | profile entersFormalFlow=false |");
            builder.AppendLine($"| `DevChapterContentPool` | `{context.DevChapterPool.chapters.Count}` dev chapter profile(s) | `{context.DevChapterBenchmark.results.Count}` simulation result(s) | no formal stage / entrance / reward / drop |");
            builder.AppendLine($"| `LedgerTaskBuildHooks` | sandbox events | `{context.LedgerHook.taskGoals.Count}` goal(s), `{context.LedgerEvents.Count}` event(s), `{context.LedgerProgress.Count}` progress preview(s) | no formal task list / reward / player progress |");
            builder.AppendLine();
            builder.AppendLine("## Active Synergy Output");
            builder.AppendLine();
            builder.AppendLine("| synergyId | thresholds | sourceItems | placement | energy |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (ActiveSynergyResult synergy in context.BuildEvaluation.activeSynergies)
            {
                builder.AppendLine(
                    $"| `{Escape(synergy.synergyId)}` | `{Escape(FormatStrings(synergy.activeThresholds))}` | `{Escape(FormatStrings(synergy.sourceItems))}` | `{synergy.placementSatisfied}` | `{synergy.energySatisfied}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Benchmark Output");
            builder.AppendLine();
            builder.AppendLine("| buildId | winRate | clearTime | activeSynergies | devOnly | isEnabled | formalFlow | formalCombat |");
            builder.AppendLine("| --- | ---: | ---: | --- | --- | --- | --- | --- |");
            foreach (BuildSimulationResult result in context.Benchmark.results)
            {
                builder.AppendLine(
                    $"| `{Escape(result.buildId)}` | {result.simulatedWinRate:0.####} | {result.simulatedClearTimeSeconds:0.##} | `{Escape(FormatStrings(result.activeSynergies))}` | `{result.devOnly}` | `{result.isEnabled}` | `{result.entersFormalFlow}` | `{result.affectsFormalCombat}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildFeatureFlagMarkdown(
            FinalIntegrationDryRunContext context,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            int errors = reports.Sum(report => report.ErrorCount);

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox FeatureFlag Final Check");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(context.FeatureFlagDefaultTrue == 0 && errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{Escape(flag.Key)}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

            builder.AppendLine();
            builder.AppendLine($"Default true count: `{context.FeatureFlagDefaultTrue}`");
            builder.AppendLine();
            builder.AppendLine("Result: all flags must remain `false`; this dry run does not open any formal Build feature.");
            return builder.ToString();
        }

        private static string BuildDevOnlyMarkdown(
            FinalIntegrationDryRunContext context,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            int errors = reports.Sum(report => report.ErrorCount);

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox devOnly Final Check");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(context.NonDevOnlyConfigCount == 0 && context.EnabledConfigCount == 0 && errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Config Assets");
            builder.AppendLine();
            builder.AppendLine($"- Scanned: `{context.Configs.Count}`");
            builder.AppendLine($"- devOnly=false: `{context.NonDevOnlyConfigCount}`");
            builder.AppendLine($"- isEnabled=true: `{context.EnabledConfigCount}`");
            builder.AppendLine();
            builder.AppendLine("| configId | devOnly | isEnabled | asset |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxDevOnlyConfig config in context.Configs)
            {
                builder.AppendLine(
                    $"| `{Escape(config.configId)}` | `{config.devOnly}` | `{config.isEnabled}` | `{Escape(AssetDatabase.GetAssetPath(config))}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Runtime Preview Outputs");
            builder.AppendLine();
            builder.AppendLine("| Output | devOnly | isEnabled | Formal flags |");
            builder.AppendLine("| --- | --- | --- | --- |");
            builder.AppendLine($"| `CombatModifierBundle` | `{context.ModifierBundle.devOnly}` | `{context.ModifierBundle.isEnabled}` | affectsFormalCombat=`{context.ModifierBundle.affectsFormalCombat}` |");
            builder.AppendLine($"| `EffectEventBundle` | `{context.EventBundle.devOnly}` | `{context.EventBundle.isEnabled}` | affectsFormalCombat=`{context.EventBundle.affectsFormalCombat}` |");
            builder.AppendLine($"| `EnemyBossValidationPool` | `{context.EnemyBossPool.devOnly}` | `{context.EnemyBossPool.isEnabled}` | profiles formal leaks=`{CountEnemyBossLeaks(context.EnemyBossPool)}` |");
            builder.AppendLine($"| `DevChapterContentPool` | `{context.DevChapterPool.devOnly}` | `{context.DevChapterPool.isEnabled}` | chapter formal leaks=`{CountDevChapterLeaks(context.DevChapterPool)}` |");
            builder.AppendLine($"| `LedgerBuildTaskHook` | `{context.LedgerHook.devOnly}` | `{context.LedgerHook.isEnabled}` | ledger formal leaks=`{CountLedgerLeaks(context)}` |");
            return builder.ToString();
        }

        private static string BuildFormalLeakMarkdown(
            FinalIntegrationDryRunContext context,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            int errors = reports.Sum(report => report.ErrorCount);

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Formal Flow Leak Final Check");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(context.Leaks.Total == 0 && context.RuntimeSourceForbiddenTokenHits.Count == 0 && errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {context.Leaks.FeatureFlagDefaultTrue} | 0 |");
            builder.AppendLine($"| `nonDevOnlyConfigAssets` | {context.Leaks.NonDevOnlyConfigs} | 0 |");
            builder.AppendLine($"| `enabledConfigAssets` | {context.Leaks.EnabledConfigs} | 0 |");
            builder.AppendLine($"| `enemyBossFormalLeaks` | {context.Leaks.EnemyBossLeaks} | 0 |");
            builder.AppendLine($"| `devChapterFormalLeaks` | {context.Leaks.DevChapterLeaks} | 0 |");
            builder.AppendLine($"| `ledgerTaskFormalLeaks` | {context.Leaks.LedgerLeaks} | 0 |");
            builder.AppendLine($"| `modifierEventFormalLeaks` | {context.Leaks.ModifierEventLeaks} | 0 |");
            builder.AppendLine($"| `benchmarkFormalLeaks` | {context.Leaks.BenchmarkLeaks} | 0 |");
            builder.AppendLine($"| `runtimeSourceForbiddenTokenHits` | {context.RuntimeSourceForbiddenTokenHits.Count} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Runtime Source Scan");
            builder.AppendLine();
            builder.AppendLine("| Hit |");
            builder.AppendLine("| --- |");
            if (context.RuntimeSourceForbiddenTokenHits.Count == 0)
            {
                builder.AppendLine("| `none` |");
            }
            else
            {
                foreach (string hit in context.RuntimeSourceForbiddenTokenHits)
                {
                    builder.AppendLine($"| `{Escape(hit)}` |");
                }
            }

            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- No formal 1-10 / 2-10 mainline hookup.");
            builder.AppendLine("- No formal StageConfig, reward, drop, upgrade, save, player progress, Boss, or numeric config hookup.");
            builder.AppendLine("- No runtime UI layout write or current hand-tuned UI scene write by this dry run.");
            builder.AppendLine("- No commit, tag, push, cleanup, or asset deletion is performed.");
            return builder.ToString();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports)
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static FinalIntegrationDryRunLeaks CountLeaks(FinalIntegrationDryRunContext context)
        {
            return new FinalIntegrationDryRunLeaks
            {
                FeatureFlagDefaultTrue = context.FeatureFlagDefaultTrue,
                NonDevOnlyConfigs = context.NonDevOnlyConfigCount,
                EnabledConfigs = context.EnabledConfigCount,
                EnemyBossLeaks = CountEnemyBossLeaks(context.EnemyBossPool),
                DevChapterLeaks = CountDevChapterLeaks(context.DevChapterPool),
                LedgerLeaks = CountLedgerLeaks(context),
                ModifierEventLeaks = CountModifierLeaks(context.ModifierBundle, context.EventBundle),
                BenchmarkLeaks = CountBenchmarkLeaks(context)
            };
        }

        private static int CountEnemyBossLeaks(EnemyBossValidationPool pool)
        {
            int enemyLeaks = pool.enemies.Count(enemy =>
                !enemy.devOnly
                || enemy.isEnabled
                || enemy.entersFormalFlow
                || !enemy.simulatorReadable
                || enemy.referencesFormalEnemyPool
                || enemy.referencesFormalBossPool);
            int bossLeaks = pool.bosses.Count(boss =>
                !boss.devOnly
                || boss.isEnabled
                || boss.entersFormalFlow
                || !boss.simulatorReadable
                || boss.referencesFormalEnemyPool
                || boss.referencesFormalBossPool);
            int poolLeaks = pool.devOnly && !pool.isEnabled ? 0 : 1;
            return poolLeaks + enemyLeaks + bossLeaks;
        }

        private static int CountDevChapterLeaks(DevChapterContentPool pool)
        {
            int poolLeaks = pool.devOnly && !pool.isEnabled ? 0 : 1;
            int chapterLeaks = pool.chapters.Count(chapter =>
                !chapter.devOnly
                || chapter.isEnabled
                || !chapter.simulatorReadable
                || chapter.entersFormalFlow
                || chapter.usesFormalStageData
                || chapter.usesFormalFlowHook
                || chapter.appearsInFormalEntrance
                || chapter.usesProductReward
                || chapter.usesProductDrop);
            return poolLeaks + chapterLeaks;
        }

        private static int CountLedgerLeaks(FinalIntegrationDryRunContext context)
        {
            int hookLeaks = context.LedgerHook == null
                || !context.LedgerHook.devOnly
                || context.LedgerHook.isEnabled
                || context.LedgerHook.entersFormalFlow
                || context.LedgerHook.grantsFormalReward
                || context.LedgerHook.entersFormalTaskList
                || context.LedgerHook.readsFormalPlayerProgress
                ? 1
                : 0;
            int goalLeaks = context.LedgerHook?.taskGoals.Count(goal =>
                !goal.devOnly
                || goal.isEnabled
                || goal.entersFormalFlow
                || goal.grantsFormalReward
                || goal.entersFormalTaskList
                || goal.readsFormalPlayerProgress) ?? 1;
            int eventLeaks = context.LedgerEvents.Count(taskEvent =>
                !taskEvent.devOnly
                || taskEvent.isEnabled
                || taskEvent.entersFormalFlow
                || taskEvent.grantsFormalReward
                || taskEvent.readsFormalPlayerProgress);
            int progressLeaks = context.LedgerProgress.Count(progress =>
                !progress.devOnly
                || progress.isEnabled
                || progress.entersFormalFlow
                || progress.grantsFormalReward
                || progress.entersFormalTaskList);
            return hookLeaks + goalLeaks + eventLeaks + progressLeaks;
        }

        private static int CountModifierLeaks(CombatModifierBundle modifierBundle, EffectEventBundle eventBundle)
        {
            int bundleLeaks = modifierBundle == null
                || !modifierBundle.devOnly
                || modifierBundle.isEnabled
                || modifierBundle.affectsFormalCombat
                ? 1
                : 0;
            bundleLeaks += eventBundle == null
                || !eventBundle.devOnly
                || eventBundle.isEnabled
                || eventBundle.affectsFormalCombat
                ? 1
                : 0;
            int modifierLeaks = modifierBundle?.modifiers.Count(modifier =>
                !modifier.devOnly
                || modifier.isEnabled
                || modifier.affectsFormalCombat) ?? 1;
            int eventLeaks = eventBundle?.events.Count(taskEvent =>
                !taskEvent.devOnly
                || taskEvent.isEnabled
                || taskEvent.affectsFormalCombat) ?? 1;
            return bundleLeaks + modifierLeaks + eventLeaks;
        }

        private static int CountBenchmarkLeaks(FinalIntegrationDryRunContext context)
        {
            return CountBenchmarkResultLeaks(context.Benchmark)
                + CountBenchmarkResultLeaks(context.EnemyBossBenchmark)
                + CountBenchmarkResultLeaks(context.DevChapterBenchmark);
        }

        private static int CountBenchmarkResultLeaks(BuildSimulationBenchmarkReport benchmark)
        {
            int reportLeaks = benchmark == null
                || !benchmark.devOnly
                || benchmark.isEnabled
                || !benchmark.sandboxEstimate
                ? 1
                : 0;
            int scenarioLeaks = benchmark?.scenarios.Count(scenario =>
                !scenario.devOnly
                || scenario.isEnabled
                || scenario.entersFormalFlow
                || scenario.referencesFormalEnemyPool
                || scenario.referencesFormalBossPool
                || scenario.readsFormalPlayerData) ?? 1;
            int resultLeaks = benchmark?.results.Count(result =>
                !result.devOnly
                || result.isEnabled
                || !result.sandboxEstimate
                || result.entersFormalFlow
                || result.affectsFormalCombat
                || !result.simulatorReadable) ?? 1;
            int comparisonLeaks = benchmark?.comparisons.Count(comparison =>
                !comparison.devOnly
                || comparison.isEnabled
                || !comparison.sandboxEstimate) ?? 1;
            return reportLeaks + scenarioLeaks + resultLeaks + comparisonLeaks;
        }

        private static List<string> ScanRuntimeSourceForbiddenTokens()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            List<string> hits = new();
            foreach (string root in RuntimeSourceRoots)
            {
                string absoluteRoot = Path.Combine(projectRoot, root.Replace('/', Path.DirectorySeparatorChar));
                if (!Directory.Exists(absoluteRoot))
                {
                    continue;
                }

                foreach (string file in Directory.GetFiles(absoluteRoot, "*.cs", SearchOption.AllDirectories))
                {
                    string text = StripCommentsAndStrings(File.ReadAllText(file));
                    foreach (string token in ForbiddenRuntimeTokens)
                    {
                        if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            hits.Add($"{ToProjectRelative(file)}::{token}");
                        }
                    }
                }
            }

            return hits.OrderBy(hit => hit, StringComparer.Ordinal).ToList();
        }

        private static List<string> MergeReportIndex(IEnumerable<string> generatedReportPaths)
        {
            return (generatedReportPaths ?? Enumerable.Empty<string>())
                .Select(ToProjectRelative)
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
        }

        private static string ToProjectRelative(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalized = path.Replace('\\', '/');
            int docsIndex = normalized.IndexOf("Docs/", StringComparison.Ordinal);
            if (docsIndex >= 0)
            {
                return normalized.Substring(docsIndex);
            }

            int assetsIndex = normalized.IndexOf("Assets/", StringComparison.Ordinal);
            return assetsIndex >= 0 ? normalized.Substring(assetsIndex) : normalized;
        }

        private static string StripCommentsAndStrings(string source)
        {
            StringBuilder builder = new(source.Length);
            bool inString = false;
            bool inChar = false;
            bool inLineComment = false;
            bool inBlockComment = false;
            bool verbatimString = false;

            for (int i = 0; i < source.Length; i++)
            {
                char current = source[i];
                char next = i + 1 < source.Length ? source[i + 1] : '\0';

                if (inLineComment)
                {
                    if (current == '\n')
                    {
                        inLineComment = false;
                        builder.Append('\n');
                    }

                    continue;
                }

                if (inBlockComment)
                {
                    if (current == '*' && next == '/')
                    {
                        inBlockComment = false;
                        i++;
                    }

                    continue;
                }

                if (inString)
                {
                    if (verbatimString && current == '"' && next == '"')
                    {
                        i++;
                        continue;
                    }

                    if (current == '"' && (verbatimString || !IsEscaped(source, i)))
                    {
                        inString = false;
                        verbatimString = false;
                    }

                    continue;
                }

                if (inChar)
                {
                    if (current == '\'' && !IsEscaped(source, i))
                    {
                        inChar = false;
                    }

                    continue;
                }

                if (current == '/' && next == '/')
                {
                    inLineComment = true;
                    i++;
                    continue;
                }

                if (current == '/' && next == '*')
                {
                    inBlockComment = true;
                    i++;
                    continue;
                }

                if (current == '@' && next == '"')
                {
                    inString = true;
                    verbatimString = true;
                    i++;
                    continue;
                }

                if (current == '"')
                {
                    inString = true;
                    continue;
                }

                if (current == '\'')
                {
                    inChar = true;
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        private static bool IsEscaped(string source, int index)
        {
            int slashCount = 0;
            for (int i = index - 1; i >= 0 && source[i] == '\\'; i--)
            {
                slashCount++;
            }

            return slashCount % 2 == 1;
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private sealed class FinalIntegrationDryRunContext
        {
            public List<BuildSandboxDevOnlyConfig> Configs = new();
            public List<ItemTagConfig> ItemTags = new();
            public List<SynergyConfig> Synergies = new();
            public List<ItemShapeConfig> Shapes = new();
            public List<RarityTierConfig> Rarities = new();
            public List<AffixConfig> Affixes = new();
            public BuildSandboxLayoutSnapshot ShapeSnapshot = new();
            public BuildSandboxLayoutSnapshot SynergySnapshot = new();
            public BuildSandboxLayoutSnapshot AffixSnapshot = new();
            public BuildEvaluationResult BuildEvaluation = new();
            public AffixRarityEvaluationResult AffixEvaluation = new();
            public CombatModifierBundle ModifierBundle = new();
            public EffectEventBundle EventBundle = new();
            public BuildSimulationBenchmarkReport Benchmark = new();
            public EnemyBossValidationPool EnemyBossPool = new();
            public BuildSimulationBenchmarkReport EnemyBossBenchmark = new();
            public DevChapterContentPool DevChapterPool = new();
            public BuildSimulationBenchmarkReport DevChapterBenchmark = new();
            public LedgerBuildTaskHook LedgerHook = new();
            public List<LedgerBuildTaskEvent> LedgerEvents = new();
            public List<LedgerBuildTaskProgressPreview> LedgerProgress = new();
            public int FeatureFlagDefaultTrue;
            public int NonDevOnlyConfigCount;
            public int EnabledConfigCount;
            public FinalIntegrationDryRunLeaks Leaks = new();
            public List<string> RuntimeSourceForbiddenTokenHits = new();
        }

        private sealed class FinalIntegrationDryRunLeaks
        {
            public int FeatureFlagDefaultTrue;
            public int NonDevOnlyConfigs;
            public int EnabledConfigs;
            public int EnemyBossLeaks;
            public int DevChapterLeaks;
            public int LedgerLeaks;
            public int ModifierEventLeaks;
            public int BenchmarkLeaks;

            public int Total =>
                FeatureFlagDefaultTrue
                + NonDevOnlyConfigs
                + EnabledConfigs
                + EnemyBossLeaks
                + DevChapterLeaks
                + LedgerLeaks
                + ModifierEventLeaks
                + BenchmarkLeaks;
        }
    }
}
#endif
