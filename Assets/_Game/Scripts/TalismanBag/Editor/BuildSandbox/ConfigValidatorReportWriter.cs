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
    public static class ConfigValidatorReportWriter
    {
        public const string ConfigValidationReportPath =
            "Docs/V0.4/Reports/ConfigValidationReport.md";

        public const string SynergyCoverageReportPath =
            "Docs/V0.4/Reports/SynergyCoverageReport.md";

        public const string AffixPoolReportPath =
            "Docs/V0.4/Reports/AffixPoolReport.md";

        public const string BuildSandboxLeakCheckReportPath =
            "Docs/V0.4/Reports/BuildSandboxLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            IReadOnlyList<string> generatedReportPaths)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string configPath = Path.Combine(projectRoot, ConfigValidationReportPath);
            string synergyPath = Path.Combine(projectRoot, SynergyCoverageReportPath);
            string affixPath = Path.Combine(projectRoot, AffixPoolReportPath);
            string leakPath = Path.Combine(projectRoot, BuildSandboxLeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(configPath) ?? projectRoot);

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            List<string> reportIndex = MergeReportIndex(generatedReportPaths);

            File.WriteAllText(
                configPath,
                BuildConfigValidationMarkdown(reports, reportIndex, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                synergyPath,
                BuildSynergyCoverageMarkdown(reports, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                affixPath,
                BuildAffixPoolMarkdown(reports, errors, warnings),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckMarkdown(reports, errors, warnings),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { configPath, synergyPath, affixPath, leakPath };
        }

        private static string BuildConfigValidationMarkdown(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            IReadOnlyList<string> reportIndex,
            int errors,
            int warnings)
        {
            int totalInfo = reports.Sum(report => report.InfoCount);
            int configCount = BuildSandboxConfigValidator.CollectConfigAssets().Count;
            int disabledFlags = BuildSandboxFeatureFlags.All.Count(flag => !flag.DefaultValue);
            int enabledFlags = BuildSandboxFeatureFlags.All.Length - disabledFlags;

            StringBuilder builder = new();
            builder.AppendLine("# Config Validation Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-ConfigValidatorReport01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Total Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Info: `{totalInfo}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Editor-only BuildSandbox validation and report export.");
            builder.AppendLine("- Read-only scan of BuildSandbox configs, seed pools, preview evaluators, and leak guards.");
            builder.AppendLine("- No automatic config repair, asset deletion, formal flow hookup, UI layout write, save write, reward write, numeric write, or Boss flow write.");
            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Status");
            builder.AppendLine();
            builder.AppendLine($"- Feature flags checked: `{BuildSandboxFeatureFlags.All.Length}`");
            builder.AppendLine($"- Default false: `{disabledFlags}`");
            builder.AppendLine($"- Default true errors: `{enabledFlags}`");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{Escape(flag.Key)}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## devOnly / isEnabled Status");
            builder.AppendLine();
            IReadOnlyList<BuildSandboxDevOnlyConfig> configs = BuildSandboxConfigValidator.CollectConfigAssets();
            int nonDevOnly = configs.Count(config => !config.devOnly);
            int enabledConfigs = configs.Count(config => config.isEnabled);
            builder.AppendLine($"- BuildSandbox config assets scanned: `{configCount}`");
            builder.AppendLine($"- devOnly=false: `{nonDevOnly}`");
            builder.AppendLine($"- isEnabled=true: `{enabledConfigs}`");
            builder.AppendLine();
            builder.AppendLine("| Config | devOnly | isEnabled | Asset |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxDevOnlyConfig config in configs)
            {
                string path = AssetDatabase.GetAssetPath(config);
                builder.AppendLine(
                    $"| `{Escape(config.configId)}` | `{config.devOnly}` | `{config.isEnabled}` | `{Escape(path)}` |");
            }

            AppendValidationSummary(builder, reports);
            builder.AppendLine();
            builder.AppendLine("## Report Index");
            builder.AppendLine();
            builder.AppendLine("| Report |");
            builder.AppendLine("| --- |");
            foreach (string reportPath in reportIndex)
            {
                builder.AppendLine($"| `{Escape(ToProjectRelative(reportPath))}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Formal Flow Leak Check");
            builder.AppendLine();
            builder.AppendLine("- FeatureFlag default true: expected `0`.");
            builder.AppendLine("- devOnly=false BuildSandbox configs: expected `0`.");
            builder.AppendLine("- isEnabled=true BuildSandbox configs: expected `0`.");
            builder.AppendLine("- DevChapter / Ledger / EnemyBoss leak counters are consolidated in `BuildSandboxLeakCheckReport.md`.");
            builder.AppendLine();
            builder.AppendLine("## Next Package Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "`V0.3/V0.4-BuildSandbox-FinalIntegrationDryRun01` can enter after C# compile passes and the user confirms this report."
                : "Do not enter FinalIntegrationDryRun01 until ConfigValidatorReport01 errors are resolved under a new approved boundary.");
            return builder.ToString();
        }

        private static string BuildSynergyCoverageMarkdown(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            IReadOnlyList<ItemTagConfig> itemTags = BuildSandboxConfigValidator.CollectItemTagConfigs();
            IReadOnlyList<SynergyConfig> synergies = BuildSandboxConfigValidator.CollectSynergyConfigs();
            IReadOnlyList<BuildArchetypeConfig> archetypes = BuildSandboxConfigValidator.CollectBuildArchetypeConfigs();
            BuildEvaluationResult evaluation =
                SynergyEvaluatorCoreValidator.BuildSampleEvaluation(out BuildSandboxLayoutSnapshot snapshot);

            HashSet<string> observedTags = new(StringComparer.Ordinal);
            foreach (ItemTagConfig config in itemTags)
            {
                foreach (string tag in config.tags ?? Enumerable.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        observedTags.Add(tag);
                    }
                }
            }

            StringBuilder builder = new();
            builder.AppendLine("# Synergy Coverage Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-ConfigValidatorReport01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine("Data label: `devOnly / disabled / coverage only / not formal flow`");
            builder.AppendLine();
            builder.AppendLine("## Coverage Counts");
            builder.AppendLine();
            builder.AppendLine($"- ItemTagConfig: `{itemTags.Count}`");
            builder.AppendLine($"- SynergyConfig: `{synergies.Count}`");
            builder.AppendLine($"- BuildArchetypeConfig: `{archetypes.Count}`");
            builder.AppendLine($"- Seed snapshot items: `{snapshot.placedItems.Count}`");
            builder.AppendLine($"- Active synergies in sample: `{evaluation.activeSynergies.Count}`");
            builder.AppendLine($"- Active thresholds in sample: `{evaluation.activeThresholds.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Required Tag Coverage");
            builder.AppendLine();
            builder.AppendLine("| Required Tag | Present |");
            builder.AppendLine("| --- | --- |");
            foreach (string tag in BuildSandboxConfigValidator.RequiredTags)
            {
                builder.AppendLine($"| {Escape(tag)} | `{observedTags.Contains(tag)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Synergy Threshold Coverage");
            builder.AppendLine();
            builder.AppendLine("| synergyId | requiredTags | thresholds | modifierOrEventTokens | devOnly | isEnabled |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (SynergyConfig synergy in synergies)
            {
                string thresholds = FormatStrings(
                    (synergy.thresholds ?? new List<SynergyThresholdConfig>())
                    .Select(threshold => threshold.pieceCount.ToString()));
                string effectTokens = FormatStrings(
                    (synergy.thresholds ?? new List<SynergyThresholdConfig>())
                    .Select(threshold => threshold.effectToken));
                builder.AppendLine(
                    $"| `{Escape(synergy.synergyId)}` | `{Escape(FormatStrings(synergy.requiredTags))}` | `{Escape(thresholds)}` | `{Escape(effectTokens)}` | `{synergy.devOnly}` | `{synergy.isEnabled}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Active Synergy Sample");
            builder.AppendLine();
            builder.AppendLine("| synergyId | matchedCount | activeThresholds | placementSatisfied | energySatisfied | sourceItems |");
            builder.AppendLine("| --- | ---: | --- | --- | --- | --- |");
            foreach (ActiveSynergyResult synergy in evaluation.activeSynergies)
            {
                builder.AppendLine(
                    $"| `{Escape(synergy.synergyId)}` | {synergy.matchedCount} | `{Escape(FormatStrings(synergy.activeThresholds))}` | `{synergy.placementSatisfied}` | `{synergy.energySatisfied}` | `{Escape(FormatStrings(synergy.sourceItems))}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildAffixPoolMarkdown(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            IReadOnlyList<RarityTierConfig> rarities = BuildSandboxConfigValidator.CollectRarityTierConfigs();
            IReadOnlyList<AffixConfig> affixes = BuildSandboxConfigValidator.CollectAffixConfigs();
            AffixRarityEvaluationResult evaluation =
                AffixRaritySandboxValidator.BuildSampleEvaluation(out BuildSandboxLayoutSnapshot snapshot);

            StringBuilder builder = new();
            builder.AppendLine("# Affix Pool Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-ConfigValidatorReport01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine("Data label: `devOnly / disabled / preview pool only / not drop, forge, wash, reward, or numeric config`");
            builder.AppendLine();
            builder.AppendLine("## Rarity Tiers");
            builder.AppendLine();
            builder.AppendLine("| rarityId | displayName | tierIndex | affixSlots | rollWeight | multiplier | devOnly | isEnabled |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: | ---: | --- | --- |");
            foreach (RarityTierConfig rarity in rarities)
            {
                builder.AppendLine(
                    $"| `{Escape(rarity.rarityId)}` | {Escape(rarity.displayName)} | {rarity.tierIndex} | {rarity.affixSlotCount} | {rarity.rollWeight} | {rarity.previewPowerMultiplier:0.###} | `{rarity.devOnly}` | `{rarity.isEnabled}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Affix Configs");
            builder.AppendLine();
            builder.AppendLine("| affixId | group | requiredTags | allowedRarities | rollRange | previewToken | devOnly | isEnabled |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (AffixConfig affix in affixes)
            {
                builder.AppendLine(
                    $"| `{Escape(affix.affixId)}` | `{Escape(affix.affixGroup)}` | `{Escape(FormatStrings(affix.requiredTags))}` | `{Escape(FormatStrings(affix.allowedRarities))}` | `{affix.minRoll}..{affix.maxRoll}` | `{Escape(affix.previewResultToken)}` | `{affix.devOnly}` | `{affix.isEnabled}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Sample Evaluation");
            builder.AppendLine();
            builder.AppendLine($"- Seed snapshot items: `{snapshot.placedItems.Count}`");
            builder.AppendLine($"- Result items: `{evaluation.itemResults.Count}`");
            builder.AppendLine($"- Rarities seen: `{Escape(FormatStrings(evaluation.rarityIds))}`");
            builder.AppendLine($"- Affixes selected: `{Escape(FormatStrings(evaluation.affixIds))}`");
            builder.AppendLine($"- Missing requirements: `{evaluation.missingRequirements.Count}`");
            builder.AppendLine();
            builder.AppendLine("| itemId | rarity | selectedAffixes | affixSlots | totalPreviewPower | sourceTags |");
            builder.AppendLine("| --- | --- | --- | ---: | ---: | --- |");
            foreach (AffixRarityItemResult item in evaluation.itemResults)
            {
                builder.AppendLine(
                    $"| `{Escape(item.itemId)}` | `{Escape(item.rarityId)}` | `{Escape(FormatStrings(item.selectedAffixes))}` | {item.affixSlotCount} | {item.totalPreviewPower} | `{Escape(FormatStrings(item.sourceTags))}` |");
            }

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildLeakCheckMarkdown(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            int errors,
            int warnings)
        {
            IReadOnlyList<BuildSandboxDevOnlyConfig> configs = BuildSandboxConfigValidator.CollectConfigAssets();
            EnemyBossValidationPool enemyBossPool = EnemyBossValidationPool.CreateDefault();
            DevChapterContentPool devChapterPool = DevChapterContentPool.CreateDefault();
            LedgerTaskBuildHooksValidator.BuildPreview(
                out LedgerBuildTaskHook ledgerHook,
                out List<LedgerBuildTaskEvent> ledgerEvents,
                out List<LedgerBuildTaskProgressPreview> ledgerProgress);
            ModifierEventBridgeValidator.BuildSamplePreview(
                out _,
                out _,
                out CombatModifierBundle modifierBundle,
                out EffectEventBundle eventBundle);
            BuildSimulationBenchmarkReport benchmark = BuildSimulationBenchmarkValidator.BuildSampleBenchmark();

            int defaultTrueFlags = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int nonDevOnlyConfigs = configs.Count(config => !config.devOnly);
            int enabledConfigs = configs.Count(config => config.isEnabled);
            int enemyBossLeaks = CountEnemyBossLeaks(enemyBossPool);
            int devChapterLeaks = CountDevChapterLeaks(devChapterPool);
            int ledgerLeaks = CountLedgerLeaks(ledgerHook, ledgerEvents, ledgerProgress);
            int modifierLeaks = CountModifierLeaks(modifierBundle, eventBundle);
            int benchmarkLeaks = benchmark.results.Count(result =>
                result.entersFormalFlow
                || result.affectsFormalCombat
                || !result.devOnly
                || result.isEnabled);

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Leak Check Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-ConfigValidatorReport01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {defaultTrueFlags} | 0 |");
            builder.AppendLine($"| `nonDevOnlyConfigAssets` | {nonDevOnlyConfigs} | 0 |");
            builder.AppendLine($"| `enabledConfigAssets` | {enabledConfigs} | 0 |");
            builder.AppendLine($"| `enemyBossFormalLeaks` | {enemyBossLeaks} | 0 |");
            builder.AppendLine($"| `devChapterFormalLeaks` | {devChapterLeaks} | 0 |");
            builder.AppendLine($"| `ledgerTaskFormalLeaks` | {ledgerLeaks} | 0 |");
            builder.AppendLine($"| `modifierEventFormalLeaks` | {modifierLeaks} | 0 |");
            builder.AppendLine($"| `simulationFormalLeaks` | {benchmarkLeaks} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Formal 1-10 / 2-10 mainline: not connected.");
            builder.AppendLine("- Formal drop / forge / wash / task / reward flow: not connected.");
            builder.AppendLine("- RunFlow / PageState / FormationState / V02RunFlowController / V02FormationGridFrame / DamageText: not modified by this report package.");
            builder.AppendLine("- SaveData / MainTrialProgressData / PlayerPrefs: not read or written by this report package.");
            builder.AppendLine("- Current formal combat UI and current formal battle-prepare interaction: not modified by this report package.");
            builder.AppendLine("- This report package does not run FinalIntegrationDryRun01.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine();
            builder.AppendLine("## Validation Summary");
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

        private static List<string> MergeReportIndex(IEnumerable<string> generatedReportPaths)
        {
            string[] required =
            {
                ConfigValidationReportPath,
                SynergyCoverageReportPath,
                EnemyBossValidationPoolReportWriter.EnemyBuildMappingReportPath,
                AffixPoolReportPath,
                BuildSandboxReportWriter.ShapePlacementReportPath,
                BuildSandboxReportWriter.GridOccupancyReportPath,
                BuildSandboxLeakCheckReportPath
            };

            return required
                .Concat((generatedReportPaths ?? Enumerable.Empty<string>()).Select(ToProjectRelative))
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
        }

        private static int CountEnemyBossLeaks(EnemyBossValidationPool pool)
        {
            int enemies = pool.enemies.Count(enemy =>
                !enemy.devOnly
                || enemy.isEnabled
                || enemy.entersFormalFlow
                || !enemy.simulatorReadable);
            int bosses = pool.bosses.Count(boss =>
                !boss.devOnly
                || boss.isEnabled
                || boss.entersFormalFlow
                || !boss.simulatorReadable);
            return enemies + bosses;
        }

        private static int CountDevChapterLeaks(DevChapterContentPool pool)
        {
            return pool.chapters.Count(chapter =>
                !chapter.devOnly
                || chapter.isEnabled
                || !chapter.simulatorReadable
                || chapter.entersFormalFlow
                || chapter.usesFormalStageData
                || chapter.usesFormalFlowHook
                || chapter.appearsInFormalEntrance
                || chapter.usesProductReward
                || chapter.usesProductDrop);
        }

        private static int CountLedgerLeaks(
            LedgerBuildTaskHook hook,
            IReadOnlyList<LedgerBuildTaskEvent> events,
            IReadOnlyList<LedgerBuildTaskProgressPreview> progress)
        {
            int hookLeaks = hook == null
                ? 1
                : (hook.devOnly
                   && !hook.isEnabled
                   && !hook.entersFormalFlow
                   && !hook.grantsFormalReward
                   && !hook.entersFormalTaskList
                   && !hook.readsFormalPlayerProgress
                    ? 0
                    : 1);
            int goalLeaks = hook?.taskGoals.Count(goal =>
                !goal.devOnly
                || goal.isEnabled
                || goal.entersFormalFlow
                || goal.grantsFormalReward
                || goal.entersFormalTaskList
                || goal.readsFormalPlayerProgress) ?? 1;
            int eventLeaks = events.Count(item =>
                !item.devOnly
                || item.isEnabled
                || item.entersFormalFlow
                || item.grantsFormalReward
                || item.readsFormalPlayerProgress);
            int progressLeaks = progress.Count(item =>
                !item.devOnly
                || item.isEnabled
                || item.entersFormalFlow
                || item.grantsFormalReward
                || item.entersFormalTaskList);
            return hookLeaks + goalLeaks + eventLeaks + progressLeaks;
        }

        private static int CountModifierLeaks(CombatModifierBundle modifierBundle, EffectEventBundle eventBundle)
        {
            int bundleLeaks = 0;
            if (modifierBundle == null
                || !modifierBundle.devOnly
                || modifierBundle.isEnabled
                || modifierBundle.affectsFormalCombat)
            {
                bundleLeaks++;
            }

            if (eventBundle == null
                || !eventBundle.devOnly
                || eventBundle.isEnabled
                || eventBundle.affectsFormalCombat)
            {
                bundleLeaks++;
            }

            int modifierLeaks = modifierBundle?.modifiers.Count(modifier =>
                !modifier.devOnly
                || modifier.isEnabled
                || modifier.affectsFormalCombat) ?? 1;
            int eventLeaks = eventBundle?.events.Count(item =>
                !item.devOnly
                || item.isEnabled
                || item.affectsFormalCombat) ?? 1;
            return bundleLeaks + modifierLeaks + eventLeaks;
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
    }
}
#endif
