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
    public static class BuildSandboxReportWriter
    {
        public const string ReportPath =
            "Docs/V0.4/Reports/BuildSandboxGuardBaselineReport.md";

        public const string SynergyDataFoundationReportPath =
            "Docs/V0.4/Reports/SynergyDataFoundationReport.md";

        public const string ShapePlacementReportPath =
            "Docs/V0.4/Reports/ShapePlacementReport.md";

        public const string GridOccupancyReportPath =
            "Docs/V0.4/Reports/GridOccupancyReport.csv";

        public const string SynergyEvaluatorCoreReportPath =
            "Docs/V0.4/Reports/SynergyEvaluatorCoreReport.md";

        public const string SynergyActivationReportPath =
            "Docs/V0.4/Reports/SynergyActivationReport.csv";

        public const string AffixRaritySandboxReportPath =
            "Docs/V0.4/Reports/AffixRaritySandboxReport.md";

        public const string AffixRarityRollReportPath =
            "Docs/V0.4/Reports/AffixRarityRollReport.csv";

        public const string ModifierEventBridgeReportPath =
            "Docs/V0.4/Reports/ModifierEventBridgeReport.md";

        public const string CombatModifierBundlePreviewPath =
            "Docs/V0.4/Reports/CombatModifierBundlePreview.csv";

        public const string EffectEventBundlePreviewPath =
            "Docs/V0.4/Reports/EffectEventBundlePreview.csv";

        public static string WriteGuardBaselineReport(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string absolutePath = Path.Combine(projectRoot, ReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? projectRoot);

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            StringBuilder builder = new();
            builder.AppendLine("# BuildSandbox Guard Baseline Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-GuardBaseline01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Feature flags default false.");
            builder.AppendLine("- devOnly configs stay devOnly=true and isEnabled=false.");
            builder.AppendLine("- Config validation is Editor-only and read-only.");
            builder.AppendLine("- UI Layout Guard scans BuildSandbox code and does not write scenes.");
            builder.AppendLine("- CoreFlow Smoke is a placeholder entry and does not enter product flow.");
            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Defaults");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{flag.Key}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

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

            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Check");
            builder.AppendLine();
            builder.AppendLine("- Current UI scenes / RectTransform / hand-tuned layout: not modified by this package.");
            builder.AppendLine("- RunFlow / PageState / FormationState: not modified by this package.");
            builder.AppendLine("- SaveData / PlayerPrefs / MainTrialProgressData: not modified by this package.");
            builder.AppendLine("- Boss / rewards / drops / numeric configs: not modified by this package.");
            builder.AppendLine("- FeatureFlag default true: none.");
            builder.AppendLine("- devOnly=false BuildSandbox configs: none when status is PASS.");
            builder.AppendLine();
            builder.AppendLine("## Next Package Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "Package 1 can enter only after C# compile passes and the user confirms this report."
                : "Package 1 must not enter until all GuardBaseline errors are resolved.");

            File.WriteAllText(absolutePath, builder.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return absolutePath;
        }

        public static string WriteSynergyDataFoundationReport(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string absolutePath = Path.Combine(projectRoot, SynergyDataFoundationReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? projectRoot);

            IReadOnlyList<ItemTagConfig> itemTags = BuildSandboxConfigValidator.CollectItemTagConfigs();
            IReadOnlyList<SynergyConfig> synergies = BuildSandboxConfigValidator.CollectSynergyConfigs();
            IReadOnlyList<BuildArchetypeConfig> archetypes =
                BuildSandboxConfigValidator.CollectBuildArchetypeConfigs();

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            HashSet<string> configuredTags = new(StringComparer.Ordinal);
            foreach (ItemTagConfig config in itemTags)
            {
                foreach (string tag in config.tags ?? Enumerable.Empty<string>())
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        configuredTags.Add(tag);
                    }
                }
            }

            StringBuilder builder = new();
            builder.AppendLine("# Synergy Data Foundation Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-SynergyDataFoundation01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Added Config Types");
            builder.AppendLine();
            builder.AppendLine("| Type | Count | Purpose |");
            builder.AppendLine("| --- | ---: | --- |");
            builder.AppendLine($"| `ItemTagConfig` | {itemTags.Count} | Item tags, category, energy cost, and rarity gates. |");
            builder.AppendLine($"| `SynergyConfig` | {synergies.Count} | Synergy ids, required tags, thresholds, and preview conditions. |");
            builder.AppendLine($"| `SynergyThresholdConfig` | {synergies.Sum(s => s.thresholds?.Count ?? 0)} | Supports 2 / 4 / 6 / 8 piece tiers. |");
            builder.AppendLine($"| `PlacementConditionConfig` | {synergies.Sum(s => s.placementConditions?.Count ?? 0) + archetypes.Sum(a => a.placementConditions?.Count ?? 0)} | Data-only placement requirements. |");
            builder.AppendLine($"| `EnergyConditionConfig` | {synergies.Sum(s => s.energyConditions?.Count ?? 0) + archetypes.Sum(a => a.energyConditions?.Count ?? 0)} | Data-only energy requirements. |");
            builder.AppendLine($"| `BuildArchetypeConfig` | {archetypes.Count} | Build archetype seeds for later simulator packages. |");
            builder.AppendLine();
            builder.AppendLine("## First-Batch Tags");
            builder.AppendLine();
            builder.AppendLine("| Tag | Present |");
            builder.AppendLine("| --- | --- |");
            foreach (string tag in BuildSandboxConfigValidator.RequiredTags)
            {
                builder.AppendLine($"| {Escape(tag)} | `{configuredTags.Contains(tag)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Synergy Threshold Support");
            builder.AppendLine();
            builder.AppendLine("| Synergy | Thresholds |");
            builder.AppendLine("| --- | --- |");
            foreach (SynergyConfig synergy in synergies)
            {
                string thresholds = string.Join(
                    ", ",
                    (synergy.thresholds ?? new List<SynergyThresholdConfig>())
                    .Select(threshold => threshold.pieceCount.ToString())
                    .Distinct());
                builder.AppendLine($"| `{Escape(synergy.synergyId)}` | `{Escape(thresholds)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Defaults");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{flag.Key}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

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

            builder.AppendLine();
            builder.AppendLine("## Isolation Check");
            builder.AppendLine();
            builder.AppendLine("- devOnly / isEnabled: every BuildSandbox config must remain `devOnly=true` and `isEnabled=false`.");
            builder.AppendLine("- FeatureFlag default true: none when this report is PASS.");
            builder.AppendLine("- Formal combat, drops, forge, UI, save, reward, boss, and numeric systems: not connected by this package.");
            builder.AppendLine("- SynergyEvaluator and item shape occupancy logic: not implemented in this package.");
            builder.AppendLine();
            builder.AppendLine("## Next Package Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "`V0.3/V0.4-BuildSandbox-ItemShapeOccupancy01` can enter after compile passes and the user confirms this report."
                : "Fix SynergyDataFoundation01 validation errors before entering ItemShapeOccupancy01.");

            File.WriteAllText(absolutePath, builder.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return absolutePath;
        }

        public static string[] WriteItemShapeOccupancyReports(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string shapeReportAbsolutePath = Path.Combine(projectRoot, ShapePlacementReportPath);
            string gridReportAbsolutePath = Path.Combine(projectRoot, GridOccupancyReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(shapeReportAbsolutePath) ?? projectRoot);

            IReadOnlyList<ItemShapeConfig> shapes = BuildSandboxConfigValidator.CollectItemShapeConfigs();
            IReadOnlyList<ShapePlacementResult> placements =
                ItemShapeOccupancyValidator.BuildSamplePlacementResults(out GridOccupancyMap map);
            BuildSandboxLayoutSnapshot snapshot = ItemShapeOccupancyValidator.BuildSampleSnapshot();

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            StringBuilder builder = new();
            builder.AppendLine("# Shape Placement Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-ItemShapeOccupancy01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Pure BuildSandbox data and placement validation only.");
            builder.AppendLine("- No formal combat, drop, forge, save, boss, reward, numeric, or product-flow connection.");
            builder.AppendLine("- Energy relation is reported as a sandbox placeholder until a later evaluator package.");
            builder.AppendLine();
            builder.AppendLine("## Shape Configs");
            builder.AppendLine();
            builder.AppendLine("| shapeId | cellCount | offsets | rotationAllowed | devOnly | isEnabled |");
            builder.AppendLine("| --- | ---: | --- | --- | --- | --- |");
            foreach (ItemShapeConfig shape in shapes)
            {
                builder.AppendLine(
                    $"| `{Escape(shape.shapeId)}` | {shape.cellCount} | `{Escape(FormatCells(shape.occupiedOffsets))}` | `{shape.rotationAllowed}` | `{shape.devOnly}` | `{shape.isEnabled}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Placement Samples");
            builder.AppendLine();
            builder.AppendLine("| itemId | shapeId | anchorCell | occupiedCells | isValid | invalidReason | adjacentItems | energyConnected |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (ShapePlacementResult placement in placements)
            {
                builder.AppendLine(
                    $"| `{Escape(placement.ItemId)}` | `{Escape(placement.ShapeId)}` | `{Escape(placement.AnchorCell.ToString())}` | `{Escape(FormatCells(placement.OccupiedCells))}` | `{placement.IsValid}` | `{placement.InvalidReason}` | `{Escape(FormatStrings(placement.AdjacentItems))}` | `{placement.EnergyConnectionNote}:{placement.EnergyConnected}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Sandbox Snapshot");
            builder.AppendLine();
            builder.AppendLine($"- placedItems: `{snapshot.placedItems.Count}`");
            builder.AppendLine("- Every valid placement snapshot carries occupiedCells for later read-only evaluator packages.");
            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Defaults");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{flag.Key}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

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

            builder.AppendLine();
            builder.AppendLine("## Isolation Check");
            builder.AppendLine();
            builder.AppendLine("- FeatureFlag default true: none when this report is PASS.");
            builder.AppendLine("- Shape seed configs: `devOnly=true` and `isEnabled=false`.");
            builder.AppendLine("- Shape rotation feature flag remains false; first-batch seeds do not require rotation.");
            builder.AppendLine("- This package does not implement SynergyEvaluator deep calculation.");
            builder.AppendLine();
            builder.AppendLine("## Next Package Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "`V0.3/V0.4-BuildSandbox-SynergyEvaluatorCore01` can enter only after compile passes and the user confirms this report."
                : "Fix ItemShapeOccupancy01 validation errors before entering SynergyEvaluatorCore01.");

            StringBuilder csv = new();
            csv.AppendLine("itemId,shapeId,anchorCell,occupiedCells,isValid,invalidReason,adjacentItems,energyConnected");
            foreach (ShapePlacementResult placement in placements)
            {
                csv.AppendLine(Csv(
                    placement.ItemId,
                    placement.ShapeId,
                    placement.AnchorCell.ToString(),
                    FormatCells(placement.OccupiedCells),
                    placement.IsValid.ToString(),
                    placement.InvalidReason.ToString(),
                    FormatStrings(placement.AdjacentItems),
                    $"{placement.EnergyConnectionNote}:{placement.EnergyConnected}"));
            }

            csv.AppendLine();
            csv.AppendLine("occupiedCell,itemId");
            foreach (KeyValuePair<ItemShapeCell, string> entry in map.OccupiedCells
                         .OrderBy(entry => entry.Key.y)
                         .ThenBy(entry => entry.Key.x))
            {
                csv.AppendLine(Csv(entry.Key.ToString(), entry.Value));
            }

            File.WriteAllText(shapeReportAbsolutePath, builder.ToString(), new UTF8Encoding(false));
            File.WriteAllText(gridReportAbsolutePath, csv.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { shapeReportAbsolutePath, gridReportAbsolutePath };
        }

        public static string[] WriteSynergyEvaluatorCoreReports(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string coreReportAbsolutePath = Path.Combine(projectRoot, SynergyEvaluatorCoreReportPath);
            string activationReportAbsolutePath = Path.Combine(projectRoot, SynergyActivationReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(coreReportAbsolutePath) ?? projectRoot);

            BuildEvaluationResult evaluation =
                SynergyEvaluatorCoreValidator.BuildSampleEvaluation(out BuildSandboxLayoutSnapshot snapshot);
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            StringBuilder builder = new();
            builder.AppendLine("# Synergy Evaluator Core Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-SynergyEvaluatorCore01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Pure BuildSandbox snapshot evaluation only.");
            builder.AppendLine("- Input is `BuildSandboxLayoutSnapshot`; output is `BuildEvaluationResult`.");
            builder.AppendLine("- No formal combat, UI, RunFlow, PageState, FormationState, save, reward, boss, or numeric connection.");
            builder.AppendLine("- Feature flags remain default false; seed configs remain devOnly and disabled.");
            builder.AppendLine();
            builder.AppendLine("## Input Snapshot");
            builder.AppendLine();
            builder.AppendLine("| itemId | tags | occupiedCells | isPowered | energySourceId |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (BuildSandboxPlacedItemSnapshot item in snapshot.placedItems)
            {
                builder.AppendLine(
                    $"| `{Escape(item.itemId)}` | `{Escape(FormatStrings(item.tags))}` | `{Escape(FormatCells(item.occupiedCells))}` | `{item.isPowered}` | `{Escape(item.energySourceId)}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Active Synergies");
            builder.AppendLine();
            builder.AppendLine("| synergyId | activeThresholds | sourceItems | placementSatisfied | energySatisfied | nextThresholdHint |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (ActiveSynergyResult synergy in evaluation.activeSynergies)
            {
                builder.AppendLine(
                    $"| `{Escape(synergy.synergyId)}` | `{Escape(FormatStrings(synergy.activeThresholds))}` | `{Escape(FormatStrings(synergy.sourceItems))}` | `{synergy.placementSatisfied}` | `{synergy.energySatisfied}` | `{Escape(FormatHint(synergy.nextThresholdHint))}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Missing Requirements");
            builder.AppendLine();
            builder.AppendLine("| synergyId | requirementId | type | requiredTag | requiredCount | actualCount | detail |");
            builder.AppendLine("| --- | --- | --- | --- | ---: | ---: | --- |");
            foreach (SynergyRequirementResult requirement in evaluation.missingRequirements)
            {
                builder.AppendLine(
                    $"| `{Escape(requirement.synergyId)}` | `{Escape(requirement.requirementId)}` | `{Escape(requirement.requirementType)}` | `{Escape(requirement.requiredTag)}` | {requirement.requiredCount} | {requirement.actualCount} | {Escape(requirement.detail)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## BuildEvaluationResult");
            builder.AppendLine();
            builder.AppendLine($"- activeSynergies: `{evaluation.activeSynergies.Count}`");
            builder.AppendLine($"- activeThresholds: `{FormatStrings(evaluation.activeThresholds)}`");
            builder.AppendLine($"- sourceItems: `{FormatStrings(evaluation.sourceItems)}`");
            builder.AppendLine($"- placementSatisfied: `{evaluation.placementSatisfied}`");
            builder.AppendLine($"- energySatisfied: `{evaluation.energySatisfied}`");
            builder.AppendLine($"- missingRequirements: `{evaluation.missingRequirements.Count}`");
            builder.AppendLine($"- nextThresholdHint: `{FormatHint(evaluation.nextThresholdHint)}`");
            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Defaults");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{flag.Key}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

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

            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Check");
            builder.AppendLine();
            builder.AppendLine("- Formal combat connection: not touched.");
            builder.AppendLine("- Current UI scene / RectTransform / hand-tuned layout: not touched.");
            builder.AppendLine("- RunFlow / PageState / FormationState / V02RunFlowController / DamageText: not touched.");
            builder.AppendLine("- SaveData / PlayerPrefs / MainTrialProgressData: not touched.");
            builder.AppendLine("- Boss / rewards / drops / numeric configs: not touched.");
            builder.AppendLine("- AffixRaritySandbox and ModifierEventBridge: not implemented in this package.");
            builder.AppendLine();
            builder.AppendLine("## Next Package Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "`V0.3/V0.4-BuildSandbox-AffixRaritySandbox01` can enter only after compile passes and the user confirms this report."
                : "Fix SynergyEvaluatorCore01 validation errors before entering AffixRaritySandbox01.");

            StringBuilder csv = new();
            csv.AppendLine("testBuild,synergyId,activeThresholds,sourceItems,placementSatisfied,energySatisfied,missingRequirements,nextThresholdHint");
            foreach (ActiveSynergyResult synergy in evaluation.activeSynergies)
            {
                csv.AppendLine(Csv(
                    "seed_synergy_evaluator_core",
                    synergy.synergyId,
                    FormatStrings(synergy.activeThresholds),
                    FormatStrings(synergy.sourceItems),
                    synergy.placementSatisfied.ToString(),
                    synergy.energySatisfied.ToString(),
                    evaluation.missingRequirements.Count(requirement => requirement.synergyId == synergy.synergyId).ToString(),
                    FormatHint(synergy.nextThresholdHint)));
            }

            csv.AppendLine();
            csv.AppendLine("testBuild,itemId,tags,occupiedCells,isPowered,energySourceId");
            foreach (BuildSandboxPlacedItemSnapshot item in snapshot.placedItems)
            {
                csv.AppendLine(Csv(
                    "seed_synergy_evaluator_core",
                    item.itemId,
                    FormatStrings(item.tags),
                    FormatCells(item.occupiedCells),
                    item.isPowered.ToString(),
                    item.energySourceId));
            }

            File.WriteAllText(coreReportAbsolutePath, builder.ToString(), new UTF8Encoding(false));
            File.WriteAllText(activationReportAbsolutePath, csv.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { coreReportAbsolutePath, activationReportAbsolutePath };
        }

        public static string[] WriteAffixRaritySandboxReports(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string affixReportAbsolutePath = Path.Combine(projectRoot, AffixRaritySandboxReportPath);
            string rollReportAbsolutePath = Path.Combine(projectRoot, AffixRarityRollReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(affixReportAbsolutePath) ?? projectRoot);

            IReadOnlyList<RarityTierConfig> rarities = BuildSandboxConfigValidator.CollectRarityTierConfigs();
            IReadOnlyList<AffixConfig> affixes = BuildSandboxConfigValidator.CollectAffixConfigs();
            AffixRarityEvaluationResult evaluation =
                AffixRaritySandboxValidator.BuildSampleEvaluation(out BuildSandboxLayoutSnapshot snapshot);
            int errors = reports.Sum(report => report.ErrorCount);

            StringBuilder builder = new();
            builder.AppendLine("# Affix Rarity Sandbox Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-AffixRaritySandbox01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Pure BuildSandbox rarity and affix preview only.");
            builder.AppendLine("- Input is `BuildSandboxLayoutSnapshot`; output is `AffixRarityEvaluationResult`.");
            builder.AppendLine("- Rarity scaffold covers white / green / blue / purple / orange.");
            builder.AppendLine("- Orange-only preview seeds include core affix and bond +1 affix.");
            builder.AppendLine("- No formal drop, reward, forge, combat modifier, UI, RunFlow, save, boss, or numeric connection.");
            builder.AppendLine("- Feature flags remain default false; seed configs remain devOnly and disabled.");
            builder.AppendLine();
            builder.AppendLine("## Rarity Tiers");
            builder.AppendLine();
            builder.AppendLine("| rarityId | tierIndex | affixSlotCount | rollWeight | previewPowerMultiplier | devOnly | isEnabled |");
            builder.AppendLine("| --- | ---: | ---: | ---: | ---: | --- | --- |");
            foreach (RarityTierConfig rarity in rarities)
            {
                builder.AppendLine(
                    $"| `{Escape(rarity.rarityId)}` | {rarity.tierIndex} | {rarity.affixSlotCount} | {rarity.rollWeight} | {rarity.previewPowerMultiplier:0.##} | `{rarity.devOnly}` | `{rarity.isEnabled}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Affix Seeds");
            builder.AppendLine();
            builder.AppendLine("| affixId | group | requiredTags | allowedRarities | rollRange | previewResultToken | devOnly | isEnabled |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- | --- |");
            foreach (AffixConfig affix in affixes)
            {
                builder.AppendLine(
                    $"| `{Escape(affix.affixId)}` | `{Escape(affix.affixGroup)}` | `{Escape(FormatStrings(affix.requiredTags))}` | `{Escape(FormatStrings(affix.allowedRarities))}` | `{affix.minRoll}-{affix.maxRoll}` | `{Escape(affix.previewResultToken)}` | `{affix.devOnly}` | `{affix.isEnabled}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Input Snapshot");
            builder.AppendLine();
            builder.AppendLine("| itemId | rarity | affixList | tags | occupiedCells |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (BuildSandboxPlacedItemSnapshot item in snapshot.placedItems)
            {
                builder.AppendLine(
                    $"| `{Escape(item.itemId)}` | `{Escape(item.rarity)}` | `{Escape(FormatStrings(item.affixList))}` | `{Escape(FormatStrings(item.tags))}` | `{Escape(FormatCells(item.occupiedCells))}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Evaluation Result");
            builder.AppendLine();
            builder.AppendLine("| itemId | rarity | selectedAffixes | slots | totalPreviewPower | sourceTags |");
            builder.AppendLine("| --- | --- | --- | ---: | ---: | --- |");
            foreach (AffixRarityItemResult item in evaluation.itemResults)
            {
                builder.AppendLine(
                    $"| `{Escape(item.itemId)}` | `{Escape(item.rarityId)}` | `{Escape(FormatStrings(item.selectedAffixes))}` | {item.affixSlotCount} | {item.totalPreviewPower} | `{Escape(FormatStrings(item.sourceTags))}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Missing Requirements");
            builder.AppendLine();
            builder.AppendLine("| itemId | type | requiredId | satisfied | detail |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (AffixRarityRequirementResult requirement in evaluation.missingRequirements)
            {
                builder.AppendLine(
                    $"| `{Escape(requirement.itemId)}` | `{Escape(requirement.requirementType)}` | `{Escape(requirement.requiredId)}` | `{requirement.satisfied}` | {Escape(requirement.detail)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Defaults");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{flag.Key}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

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

            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Check");
            builder.AppendLine();
            builder.AppendLine("- Formal combat connection: not touched.");
            builder.AppendLine("- Current UI scene / RectTransform / hand-tuned layout: not touched.");
            builder.AppendLine("- RunFlow / PageState / FormationState / V02RunFlowController / DamageText: not touched.");
            builder.AppendLine("- SaveData / PlayerPrefs / MainTrialProgressData: not touched.");
            builder.AppendLine("- Boss / rewards / drops / numeric configs: not touched.");
            builder.AppendLine("- ModifierEventBridge: not implemented in this package.");
            builder.AppendLine();
            builder.AppendLine("## Next Package Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "`V0.3/V0.4-BuildSandbox-ModifierEventBridge01` can enter only after compile passes and the user confirms this report."
                : "Fix AffixRaritySandbox01 validation errors before entering ModifierEventBridge01.");

            StringBuilder csv = new();
            csv.AppendLine("testBuild,itemId,rarity,selectedAffixes,affixSlotCount,totalPreviewPower,sourceTags");
            foreach (AffixRarityItemResult item in evaluation.itemResults)
            {
                csv.AppendLine(Csv(
                    "seed_affix_rarity_sandbox",
                    item.itemId,
                    item.rarityId,
                    FormatStrings(item.selectedAffixes),
                    item.affixSlotCount.ToString(),
                    item.totalPreviewPower.ToString(),
                    FormatStrings(item.sourceTags)));
            }

            File.WriteAllText(affixReportAbsolutePath, builder.ToString(), new UTF8Encoding(false));
            File.WriteAllText(rollReportAbsolutePath, csv.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { affixReportAbsolutePath, rollReportAbsolutePath };
        }

        public static string[] WriteModifierEventBridgeReports(
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string bridgeReportAbsolutePath = Path.Combine(projectRoot, ModifierEventBridgeReportPath);
            string modifierPreviewAbsolutePath = Path.Combine(projectRoot, CombatModifierBundlePreviewPath);
            string eventPreviewAbsolutePath = Path.Combine(projectRoot, EffectEventBundlePreviewPath);
            Directory.CreateDirectory(Path.GetDirectoryName(bridgeReportAbsolutePath) ?? projectRoot);

            ModifierEventBridgeValidator.BuildSamplePreview(
                out BuildEvaluationResult buildEvaluation,
                out AffixRarityEvaluationResult affixEvaluation,
                out CombatModifierBundle modifierBundle,
                out EffectEventBundle eventBundle);

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);

            StringBuilder builder = new();
            builder.AppendLine("# Modifier Event Bridge Report");
            builder.AppendLine();
            builder.AppendLine("Package: `V0.3/V0.4-BuildSandbox-ModifierEventBridge01`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Pure BuildSandbox bridge preview only.");
            builder.AppendLine("- Inputs are `BuildEvaluationResult` and `AffixRarityEvaluationResult`.");
            builder.AppendLine("- Outputs are `CombatModifierBundle` and `EffectEventBundle` preview data.");
            builder.AppendLine("- No formal combat, UI, RunFlow, PageState, FormationState, save, reward, boss, or numeric connection.");
            builder.AppendLine("- Feature flags remain default false; preview outputs remain devOnly and disabled.");
            builder.AppendLine();
            builder.AppendLine("## Input BuildEvaluationResult Summary");
            builder.AppendLine();
            builder.AppendLine($"- activeSynergies: `{buildEvaluation.activeSynergies.Count}`");
            builder.AppendLine($"- activeThresholds: `{FormatStrings(buildEvaluation.activeThresholds)}`");
            builder.AppendLine($"- sourceItems: `{FormatStrings(buildEvaluation.sourceItems)}`");
            builder.AppendLine($"- placementSatisfied: `{buildEvaluation.placementSatisfied}`");
            builder.AppendLine($"- energySatisfied: `{buildEvaluation.energySatisfied}`");
            builder.AppendLine($"- missingRequirements: `{buildEvaluation.missingRequirements.Count}`");
            builder.AppendLine($"- nextThresholdHint: `{FormatHint(buildEvaluation.nextThresholdHint)}`");
            builder.AppendLine();
            builder.AppendLine("## Active Synergy Inputs");
            builder.AppendLine();
            builder.AppendLine("| synergyId | activeThresholds | sourceItems | placementSatisfied | energySatisfied |");
            builder.AppendLine("| --- | --- | --- | --- | --- |");
            foreach (ActiveSynergyResult synergy in buildEvaluation.activeSynergies)
            {
                builder.AppendLine(
                    $"| `{Escape(synergy.synergyId)}` | `{Escape(FormatStrings(synergy.activeThresholds))}` | `{Escape(FormatStrings(synergy.sourceItems))}` | `{synergy.placementSatisfied}` | `{synergy.energySatisfied}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Affix Preview Input Summary");
            builder.AppendLine();
            builder.AppendLine($"- itemResults: `{affixEvaluation.itemResults.Count}`");
            builder.AppendLine($"- rarityIds: `{FormatStrings(affixEvaluation.rarityIds)}`");
            builder.AppendLine($"- affixIds: `{FormatStrings(affixEvaluation.affixIds)}`");
            builder.AppendLine($"- missingRequirements: `{affixEvaluation.missingRequirements.Count}`");
            builder.AppendLine();
            builder.AppendLine("## Affix Item Inputs");
            builder.AppendLine();
            builder.AppendLine("| itemId | rarity | selectedAffixes | totalPreviewPower | sourceTags |");
            builder.AppendLine("| --- | --- | --- | ---: | --- |");
            foreach (AffixRarityItemResult item in affixEvaluation.itemResults)
            {
                builder.AppendLine(
                    $"| `{Escape(item.itemId)}` | `{Escape(item.rarityId)}` | `{Escape(FormatStrings(item.selectedAffixes))}` | {item.totalPreviewPower} | `{Escape(FormatStrings(item.sourceTags))}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Reserved Modifier Types");
            builder.AppendLine();
            builder.AppendLine("| modifierType | supported | generatedInSample |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (string modifierType in ModifierEventBridge.SupportedModifierTypes)
            {
                bool generated = modifierBundle.modifiers.Any(modifier => modifier.modifierType == modifierType);
                builder.AppendLine($"| `{Escape(modifierType)}` | `true` | `{generated}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Reserved Event Types");
            builder.AppendLine();
            builder.AppendLine("| eventType | supported | generatedInSample |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (string eventType in ModifierEventBridge.SupportedEventTypes)
            {
                bool generated = eventBundle.events.Any(eventPreview => eventPreview.eventType == eventType);
                builder.AppendLine($"| `{Escape(eventType)}` | `true` | `{generated}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## CombatModifierBundle Preview");
            builder.AppendLine();
            builder.AppendLine($"- sourceBuildId: `{Escape(modifierBundle.sourceBuildId)}`");
            builder.AppendLine($"- devOnly: `{modifierBundle.devOnly}`");
            builder.AppendLine($"- isEnabled: `{modifierBundle.isEnabled}`");
            builder.AppendLine($"- affectsFormalCombat: `{modifierBundle.affectsFormalCombat}`");
            builder.AppendLine($"- modifiers: `{modifierBundle.modifiers.Count}`");
            builder.AppendLine();
            builder.AppendLine("| modifierType | previewValue | sourceSynergy | sourceThreshold | sourceAffix | sourceItem | affectsFormalCombat |");
            builder.AppendLine("| --- | ---: | --- | --- | --- | --- | --- |");
            foreach (BuildModifierPreview modifier in modifierBundle.modifiers)
            {
                builder.AppendLine(
                    $"| `{Escape(modifier.modifierType)}` | {modifier.previewValue:0.###} | `{Escape(modifier.sourceSynergy)}` | `{Escape(modifier.sourceThreshold)}` | `{Escape(modifier.sourceAffix)}` | `{Escape(modifier.sourceItem)}` | `{modifier.affectsFormalCombat}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## EffectEventBundle Preview");
            builder.AppendLine();
            builder.AppendLine($"- sourceBuildId: `{Escape(eventBundle.sourceBuildId)}`");
            builder.AppendLine($"- devOnly: `{eventBundle.devOnly}`");
            builder.AppendLine($"- isEnabled: `{eventBundle.isEnabled}`");
            builder.AppendLine($"- affectsFormalCombat: `{eventBundle.affectsFormalCombat}`");
            builder.AppendLine($"- events: `{eventBundle.events.Count}`");
            builder.AppendLine();
            builder.AppendLine("| eventType | trigger | sourceSynergy | sourceThreshold | sourceAffix | sourceItem | affectsFormalCombat |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (BuildEventPreview eventPreview in eventBundle.events)
            {
                builder.AppendLine(
                    $"| `{Escape(eventPreview.eventType)}` | `{Escape(eventPreview.trigger)}` | `{Escape(eventPreview.sourceSynergy)}` | `{Escape(eventPreview.sourceThreshold)}` | `{Escape(eventPreview.sourceAffix)}` | `{Escape(eventPreview.sourceItem)}` | `{eventPreview.affectsFormalCombat}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## FeatureFlag Defaults");
            builder.AppendLine();
            builder.AppendLine("| Flag | Default | Scope |");
            builder.AppendLine("| --- | --- | --- |");
            foreach (BuildSandboxFeatureFlagDefinition flag in BuildSandboxFeatureFlags.All)
            {
                builder.AppendLine($"| `{flag.Key}` | `{flag.DefaultValue}` | {Escape(flag.Scope)} |");
            }

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

            builder.AppendLine();
            builder.AppendLine("## Forbidden Scope Check");
            builder.AppendLine();
            builder.AppendLine("- Formal combat connection: not touched; `affectsFormalCombat=false` on every preview output.");
            builder.AppendLine("- Current UI scene / RectTransform / hand-tuned layout: not touched.");
            builder.AppendLine("- RunFlow / PageState / FormationState / V02RunFlowController / V02FormationGridFrame / DamageText: not touched.");
            builder.AppendLine("- SaveData / PlayerPrefs / MainTrialProgressData: not touched.");
            builder.AppendLine("- Boss / rewards / drops / numeric configs: not touched.");
            builder.AppendLine("- BuildSimulationBenchmark batch simulator: not implemented in this package.");
            builder.AppendLine();
            builder.AppendLine("## Next Package Gate");
            builder.AppendLine();
            builder.AppendLine(errors == 0
                ? "`V0.3/V0.4-BuildSandbox-BuildSimulationBenchmark01` can enter only after compile passes, this report is reviewed, and the user confirms this package."
                : "Fix ModifierEventBridge01 validation errors before entering BuildSimulationBenchmark01.");

            StringBuilder modifierCsv = new();
            modifierCsv.AppendLine("testBuild,modifierType,previewValue,sourceSynergy,sourceThreshold,sourceAffix,sourceItem,devOnly,isEnabled,affectsFormalCombat,previewSummary");
            foreach (BuildModifierPreview modifier in modifierBundle.modifiers)
            {
                modifierCsv.AppendLine(Csv(
                    modifierBundle.sourceBuildId,
                    modifier.modifierType,
                    modifier.previewValue.ToString("0.###"),
                    modifier.sourceSynergy,
                    modifier.sourceThreshold,
                    modifier.sourceAffix,
                    modifier.sourceItem,
                    modifier.devOnly.ToString(),
                    modifier.isEnabled.ToString(),
                    modifier.affectsFormalCombat.ToString(),
                    modifier.previewSummary));
            }

            StringBuilder eventCsv = new();
            eventCsv.AppendLine("testBuild,eventType,trigger,sourceSynergy,sourceThreshold,sourceAffix,sourceItem,devOnly,isEnabled,affectsFormalCombat,previewPayload");
            foreach (BuildEventPreview eventPreview in eventBundle.events)
            {
                eventCsv.AppendLine(Csv(
                    eventBundle.sourceBuildId,
                    eventPreview.eventType,
                    eventPreview.trigger,
                    eventPreview.sourceSynergy,
                    eventPreview.sourceThreshold,
                    eventPreview.sourceAffix,
                    eventPreview.sourceItem,
                    eventPreview.devOnly.ToString(),
                    eventPreview.isEnabled.ToString(),
                    eventPreview.affectsFormalCombat.ToString(),
                    eventPreview.previewPayload));
            }

            File.WriteAllText(bridgeReportAbsolutePath, builder.ToString(), new UTF8Encoding(false));
            File.WriteAllText(modifierPreviewAbsolutePath, modifierCsv.ToString(), new UTF8Encoding(false));
            File.WriteAllText(eventPreviewAbsolutePath, eventCsv.ToString(), new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return new[] { bridgeReportAbsolutePath, modifierPreviewAbsolutePath, eventPreviewAbsolutePath };
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
        }

        private static string FormatCells(IEnumerable<ItemShapeCell> cells)
        {
            return string.Join(";", (cells ?? Enumerable.Empty<ItemShapeCell>()).Select(cell => cell.ToString()));
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
        }

        private static string FormatHint(NextThresholdHint hint)
        {
            if (hint == null || !hint.hasNextThreshold)
            {
                return "none";
            }

            return $"{hint.synergyId}:{hint.currentPieceCount}->{hint.nextPieceCount} missing={hint.missingPieceCount}";
        }

        private static string EscapeCsv(string value)
        {
            string normalized = value ?? string.Empty;
            if (normalized.Contains(",") || normalized.Contains("\"") || normalized.Contains("\n") || normalized.Contains("\r"))
            {
                return $"\"{normalized.Replace("\"", "\"\"")}\"";
            }

            return normalized;
        }
    }
}
#endif
