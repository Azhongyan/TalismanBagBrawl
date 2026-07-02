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
    public static class BattlePageViewAdapterReportWriter
    {
        public const string MainReportPath = "Docs/V0.4/Reports/BattlePageViewAdapterReport.md";
        public const string SpecReportPath = "Docs/V0.4/Reports/BattlePageViewSpecReport.csv";
        public const string UiReuseSpecReportPath = "Docs/V0.4/Reports/BattleUiReuseSpecReport.csv";
        public const string PlayerHintMaskingSpecReportPath = "Docs/V0.4/Reports/PlayerHintMaskingSpecReport.md";
        public const string LeakCheckReportPath = "Docs/V0.4/Reports/BattlePageViewAdapterLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePageViewAdapter adapter = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattlePageViewAdapter safeAdapter = adapter ?? BattlePageViewAdapterValidator.BuildDefaultAdapter();
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string specPath = Path.Combine(projectRoot, SpecReportPath);
            string uiReusePath = Path.Combine(projectRoot, UiReuseSpecReportPath);
            string playerMaskingPath = Path.Combine(projectRoot, PlayerHintMaskingSpecReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safeAdapter);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safeAdapter, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                specPath,
                BuildSpecCsv(safeAdapter),
                new UTF8Encoding(false));
            File.WriteAllText(
                uiReusePath,
                BuildBattleUiReuseSpecCsv(safeAdapter),
                new UTF8Encoding(false));
            File.WriteAllText(
                playerMaskingPath,
                BuildPlayerHintMaskingReport(safeReports, safeAdapter, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safeAdapter, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, specPath, uiReusePath, playerMaskingPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePageViewAdapter adapter,
            int errors,
            int warnings,
            int leakCount)
        {
            BattlePageViewSpec spec = adapter?.spec ?? BattlePageViewSpec.CreateDefault();
            IReadOnlyList<BattlePageViewSpecSection> sections = spec.BuildSections();
            StringBuilder builder = new();
            builder.AppendLine("# Battle Page View Adapter Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePageViewAdapterValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Creates a read-only visual language adapter for the V04 BattleSandboxPreview scene.");
            builder.AppendLine("- Reuses battle prepare specifications, naming, and interaction language without reusing formal GameObjects.");
            builder.AppendLine($"- Does not write V02/V03 scenes, formal UI, {BattlePageViewFormalReferenceKeys.FormalLayoutFrameKind} values, formal save data, or formal battle flow.");
            builder.AppendLine();
            builder.AppendLine("## Adapter Isolation");
            builder.AppendLine();
            builder.AppendLine($"- devOnly: `{adapter?.devOnly}`");
            builder.AppendLine($"- isEnabled: `{adapter?.isEnabled}`");
            builder.AppendLine($"- readsFormalSaveData: `{adapter?.readsFormalSaveData}`");
            builder.AppendLine($"- writesFormalScene: `{adapter?.writesFormalScene}`");
            builder.AppendLine($"- writesFormalUi: `{adapter?.writesFormalUi}`");
            builder.AppendLine($"- setParentsFormalUi: `{adapter?.setParentsFormalUi}`");
            builder.AppendLine($"- overridesFormalLayoutFrame: `{adapter?.overridesFormalLayoutFrame}`");
            builder.AppendLine($"- touchesV02FormationGridFrame: `{adapter?.touchesV02FormationGridFrame}`");
            builder.AppendLine($"- touchesFormalPrepareController: `{adapter?.touchesFormalPrepareController}`");
            builder.AppendLine($"- connectsFormalBattle: `{adapter?.connectsFormalBattle}`");
            builder.AppendLine($"- FeatureFlags all disabled: `{BuildSandboxFeatureFlags.AreAllDefaultsDisabled()}`");
            builder.AppendLine();
            builder.AppendLine("## Output Specs");
            builder.AppendLine();
            builder.AppendLine($"- Output spec count: `{sections.Count}`");
            builder.AppendLine($"- Reference mode: `{Escape(spec.referenceMode)}`");
            builder.AppendLine($"- Reuses formal GameObjects: `{spec.reusesFormalGameObjects}`");
            builder.AppendLine($"- Writes formal UI: `{spec.writesFormalUi}`");
            builder.AppendLine();
            builder.AppendLine("| Section | Preview Slot | Formal Reference | Reference Only | Writes Formal UI | Contract |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- |");
            foreach (BattlePageViewSpecSection section in sections)
            {
                builder.AppendLine(
                    $"| `{Escape(section.SectionId)}` | `{Escape(section.PreviewSlotName)}` | `{Escape(section.FormalReferenceName)}` | `{section.ReferenceOnly}` | `{section.CanWriteFormalUi}` | {Escape(section.Contract)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Battle Prepare Visual Language");
            builder.AppendLine();
            builder.AppendLine($"- BoardGrid: `{spec.boardGrid.width}x{spec.boardGrid.height}` via `{Escape(spec.boardGrid.previewSlotName)}`.");
            builder.AppendLine($"- ItemTray: `{spec.itemTray.width}x{spec.itemTray.height}`, columns=`{spec.itemTray.columnCount}`, rows=`{spec.itemTray.rowCount}`, visibleRows=`{spec.itemTray.visibleRowCount}`.");
            builder.AppendLine($"- ItemTray slots: `{spec.itemTray.slotCount}`, slotSize=`{spec.itemTray.slotWidth}x{spec.itemTray.slotHeight}`, spacing=`{spec.itemTray.slotSpacingX}x{spec.itemTray.slotSpacingY}`.");
            builder.AppendLine($"- Categories: `{string.Join(" / ", spec.categoryTabs.displayLabels ?? new List<string>())}`.");
            builder.AppendLine($"- DataPanelDock slots: `{string.Join(" / ", spec.dataPanelDock.bindingSlotNames ?? new List<string>())}`.");
            builder.AppendLine();
            builder.AppendLine("## UI Reuse And Masking");
            builder.AppendLine();
            builder.AppendLine($"- Battle UI reuse channels: `{spec.battleUiReuse?.recommendations?.Count ?? 0}`.");
            builder.AppendLine($"- Developer tuning fields: `{spec.developerTuningPanel?.fields?.Count ?? 0}`.");
            builder.AppendLine($"- Player hint masking rules: `{spec.playerHintMasking?.maskingRules?.Count ?? 0}`.");
            builder.AppendLine($"- Player UI Chinese-only: `{spec.playerHintMasking?.playerUiChineseOnly}`.");
            builder.AppendLine($"- Player UI shows English stable keys: `{spec.playerHintMasking?.playerUiShowsEnglishStableKey}`.");
            builder.AppendLine($"- English stable keys limited to config/report/developer data-panel fields: `{spec.developerTuningPanel?.englishStableKeysConfigReportDevPanelOnly}`.");
            builder.AppendLine("- Player Hint Preview shows clues, combat feedback, atmosphere hints, and failure feedback only.");
            builder.AppendLine("- Developer Tuning View may show full rules, keys, thresholds, DropBias, and readiness data.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildSpecCsv(BattlePageViewAdapter adapter)
        {
            BattlePageViewSpec spec = adapter?.spec ?? BattlePageViewSpec.CreateDefault();
            StringBuilder csv = new();
            csv.AppendLine("section,specId,previewSlotName,formalReferenceName,referenceOnly,canWriteFormalUi,devOnly,isEnabled,primaryMetric,contract");
            foreach (BattlePageViewSpecSection section in spec.BuildSections())
            {
                csv.AppendLine(Csv(
                    section.SectionId,
                    spec.specId,
                    section.PreviewSlotName,
                    section.FormalReferenceName,
                    section.ReferenceOnly.ToString(),
                    section.CanWriteFormalUi.ToString(),
                    spec.devOnly.ToString(),
                    spec.isEnabled.ToString(),
                    ResolvePrimaryMetric(spec, section.SectionId),
                    section.Contract));
            }

            return csv.ToString();
        }

        private static string BuildBattleUiReuseSpecCsv(BattlePageViewAdapter adapter)
        {
            BattlePageViewSpec spec = adapter?.spec ?? BattlePageViewSpec.CreateDefault();
            StringBuilder csv = new();
            csv.AppendLine("channel,englishStableKey,chineseDisplayName,reuseSurface,reuseRecommendation,playerFacingAllowed,developerPanelAllowed,referenceOnly,canWriteFormalUi,createsDedicatedPanel,maskingNote");
            foreach (BattleUiReuseRecommendation row in spec.battleUiReuse?.recommendations ?? new List<BattleUiReuseRecommendation>())
            {
                csv.AppendLine(Csv(
                    row.channelId,
                    row.englishStableKey,
                    row.chineseDisplayName,
                    row.reuseSurface,
                    row.reuseRecommendation,
                    row.playerFacingAllowed.ToString(),
                    row.developerPanelAllowed.ToString(),
                    row.referenceOnly.ToString(),
                    row.canWriteFormalUi.ToString(),
                    row.createsDedicatedPanel.ToString(),
                    row.maskingNote));
            }

            return csv.ToString();
        }

        private static string BuildPlayerHintMaskingReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePageViewAdapter adapter,
            int errors,
            int warnings,
            int leakCount)
        {
            BattlePageViewSpec spec = adapter?.spec ?? BattlePageViewSpec.CreateDefault();
            PlayerHintMaskingSpec masking = spec.playerHintMasking ?? PlayerHintMaskingSpec.CreateDefault();
            DeveloperTuningPanelFieldSpec developerFields =
                spec.developerTuningPanel ?? DeveloperTuningPanelFieldSpec.CreateDefault();
            int maskingLeaks = CountPlayerHintMaskingLeaks(masking) + CountDeveloperTuningFieldLeaks(developerFields);

            StringBuilder builder = new();
            builder.AppendLine("# Player Hint Masking Spec Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePageViewAdapterValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 && maskingLeaks == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Masking leaks: `{maskingLeaks}`");
            builder.AppendLine();
            builder.AppendLine("## Display Boundary");
            builder.AppendLine();
            builder.AppendLine($"- Player UI Chinese-only: `{masking.playerUiChineseOnly}`");
            builder.AppendLine($"- Player UI shows English stable keys: `{masking.playerUiShowsEnglishStableKey}`");
            builder.AppendLine($"- English stable keys are limited to config/report/developer data-panel field layers: `{developerFields.englishStableKeysConfigReportDevPanelOnly}`");
            builder.AppendLine($"- Developer tuning panel may show full rules: `{masking.developerPanelMayShowFullRules}`");
            builder.AppendLine($"- Player Hint Preview shows full answers: `{masking.playerHintPreviewShowsFullAnswers}`");
            builder.AppendLine();
            builder.AppendLine("## Masking Rules");
            builder.AppendLine();
            builder.AppendLine("| English Stable Key | Chinese Display Name | Player Visible | Developer Data Panel | Config | Report | Player Chinese Hint Policy |");
            builder.AppendLine("| --- | --- | --- | --- | --- | --- | --- |");
            foreach (PlayerHintMaskingRule rule in masking.maskingRules ?? new List<PlayerHintMaskingRule>())
            {
                builder.AppendLine(
                    $"| `{Escape(rule.englishStableKey)}` | {Escape(rule.chineseDisplayName)} | `{rule.playerVisible}` | `{rule.developerDataPanelVisible}` | `{rule.configVisible}` | `{rule.reportVisible}` | {Escape(rule.playerChineseHintPolicy)} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Developer Field Layer");
            builder.AppendLine();
            builder.AppendLine("| English Stable Key | Chinese Display Name | V04 Data Panel Slot | Developer Only |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (DeveloperTuningFieldBinding field in developerFields.fields ?? new List<DeveloperTuningFieldBinding>())
            {
                builder.AppendLine(
                    $"| `{Escape(field.englishStableKey)}` | {Escape(field.chineseDisplayName)} | `{Escape(field.dataPanelSlot)}` | `{field.developerOnly}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## Player-Side Proof");
            builder.AppendLine();
            builder.AppendLine("- hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, DropBias weights, and Boss six-key full answer are masked from player UI.");
            builder.AppendLine("- Player-side copy is Chinese-only clue/feedback text; it does not include English stable keys or exact solution keys.");
            builder.AppendLine("- Full answers remain available only to config, reports, and developer data-panel fields for tuning and QA.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattlePageViewAdapter adapter,
            int errors,
            int warnings,
            int leakCount)
        {
            BattlePageViewSpec spec = adapter?.spec ?? BattlePageViewSpec.CreateDefault();
            int featureFlagDefaultTrue = BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue);
            int adapterLeaks = CountAdapterLeaks(adapter);
            int specLeaks = CountSpecLeaks(spec);
            int writableSectionLeaks = spec.BuildSections().Count(section => !section.ReferenceOnly || section.CanWriteFormalUi);
            int uiReuseLeaks = CountBattleUiReuseLeaks(spec.battleUiReuse);
            int maskingLeaks = CountPlayerHintMaskingLeaks(spec.playerHintMasking);
            int developerFieldLeaks = CountDeveloperTuningFieldLeaks(spec.developerTuningPanel);

            StringBuilder builder = new();
            builder.AppendLine("# Battle Page View Adapter Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattlePageViewAdapterValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {featureFlagDefaultTrue} | 0 |");
            builder.AppendLine($"| `adapterIsolationLeaks` | {adapterLeaks} | 0 |");
            builder.AppendLine($"| `specIsolationLeaks` | {specLeaks} | 0 |");
            builder.AppendLine($"| `writableSpecSectionLeaks` | {writableSectionLeaks} | 0 |");
            builder.AppendLine($"| `uiReuseLeaks` | {uiReuseLeaks} | 0 |");
            builder.AppendLine($"| `playerHintMaskingLeaks` | {maskingLeaks} | 0 |");
            builder.AppendLine($"| `developerFieldScopeLeaks` | {developerFieldLeaks} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Formal Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Formal SaveData / PlayerPrefs / MainTrialProgressData: not read or written.");
            builder.AppendLine($"- {BattlePageViewFormalReferenceKeys.CurrentFormationScene}: not written.");
            builder.AppendLine($"- {BattlePageViewFormalReferenceKeys.CurrentMainHomeScene}: not written.");
            builder.AppendLine($"- {BattlePageViewFormalReferenceKeys.CurrentUpgradeScene}: not written.");
            builder.AppendLine("- Scene_TalismanBag_V04_BattleSandboxPreview: not written by this adapter package.");
            builder.AppendLine($"- V02FormationGridFrame and {BattlePageViewFormalReferenceKeys.PrepareController}: not modified, not reparented, not {BattlePageViewFormalReferenceKeys.FormalLayoutFrameKind}-overridden.");
            builder.AppendLine("- Formal battle, reward, drop, Boss, upgrade, enemy, numeric configs: not connected.");
            builder.AppendLine("- Adapter output is a report/spec contract for later V04 preview packages only.");
            builder.AppendLine("- BattleHint, DamageFeedback, CombatLog, Tooltip, and BossInfo reuse guidance stays spec-only.");
            builder.AppendLine("- Player UI remains Chinese-only; English stable keys stay in config/report/developer data-panel field layers.");
            builder.AppendLine("- hardSolutionTags, requiredSynergy, requiredAffix, requiredStats, DropBias weights, and Boss six-key full answer are masked from player UI.");

            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattlePageViewAdapter adapter)
        {
            IEnumerable<BattlePageViewSpecSection> sections =
                adapter?.spec?.BuildSections() ?? Enumerable.Empty<BattlePageViewSpecSection>();

            return BuildSandboxFeatureFlags.All.Count(flag => flag.DefaultValue)
                + CountAdapterLeaks(adapter)
                + CountSpecLeaks(adapter?.spec)
                + sections.Count(section => !section.ReferenceOnly || section.CanWriteFormalUi)
                + CountBattleUiReuseLeaks(adapter?.spec?.battleUiReuse)
                + CountPlayerHintMaskingLeaks(adapter?.spec?.playerHintMasking)
                + CountDeveloperTuningFieldLeaks(adapter?.spec?.developerTuningPanel);
        }

        private static int CountAdapterLeaks(BattlePageViewAdapter adapter)
        {
            if (adapter == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!adapter.devOnly)
            {
                leaks++;
            }

            if (adapter.isEnabled)
            {
                leaks++;
            }

            if (adapter.readsFormalSaveData)
            {
                leaks++;
            }

            if (adapter.writesFormalScene)
            {
                leaks++;
            }

            if (adapter.writesFormalUi)
            {
                leaks++;
            }

            if (adapter.setParentsFormalUi)
            {
                leaks++;
            }

            if (adapter.overridesFormalLayoutFrame)
            {
                leaks++;
            }

            if (adapter.touchesV02FormationGridFrame)
            {
                leaks++;
            }

            if (adapter.touchesFormalPrepareController)
            {
                leaks++;
            }

            if (adapter.connectsFormalBattle)
            {
                leaks++;
            }

            return leaks;
        }

        private static int CountSpecLeaks(BattlePageViewSpec spec)
        {
            if (spec == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!spec.devOnly)
            {
                leaks++;
            }

            if (spec.isEnabled)
            {
                leaks++;
            }

            if (!spec.referenceOnly)
            {
                leaks++;
            }

            if (spec.reusesFormalGameObjects)
            {
                leaks++;
            }

            if (spec.writesFormalUi)
            {
                leaks++;
            }

            return leaks;
        }

        private static int CountBattleUiReuseLeaks(BattleUiReuseSpec spec)
        {
            if (spec == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!spec.referenceOnly || spec.canWriteFormalUi || spec.createsDedicatedMechanicPanels)
            {
                leaks++;
            }

            foreach (BattleUiReuseRecommendation row in spec.recommendations ?? new List<BattleUiReuseRecommendation>())
            {
                if (!row.referenceOnly || row.canWriteFormalUi || row.createsDedicatedPanel)
                {
                    leaks++;
                }
            }

            return leaks;
        }

        private static int CountPlayerHintMaskingLeaks(PlayerHintMaskingSpec spec)
        {
            if (spec == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!spec.referenceOnly
                || spec.canWriteFormalUi
                || !spec.playerUiChineseOnly
                || spec.playerUiShowsEnglishStableKey
                || !spec.developerPanelMayShowFullRules
                || spec.playerHintPreviewShowsFullAnswers)
            {
                leaks++;
            }

            foreach (PlayerHintMaskingRule rule in spec.maskingRules ?? new List<PlayerHintMaskingRule>())
            {
                if (!rule.maskRequired || rule.playerVisible || !rule.developerDataPanelVisible)
                {
                    leaks++;
                }
            }

            return leaks;
        }

        private static int CountDeveloperTuningFieldLeaks(DeveloperTuningPanelFieldSpec spec)
        {
            if (spec == null)
            {
                return 1;
            }

            int leaks = 0;
            if (!spec.referenceOnly
                || spec.canWriteFormalUi
                || !spec.playerUiChineseOnly
                || spec.playerUiShowsEnglishStableKey
                || !spec.englishStableKeysConfigReportDevPanelOnly)
            {
                leaks++;
            }

            foreach (DeveloperTuningFieldBinding field in spec.fields ?? new List<DeveloperTuningFieldBinding>())
            {
                if (!field.developerOnly)
                {
                    leaks++;
                }
            }

            return leaks;
        }

        private static string ResolvePrimaryMetric(BattlePageViewSpec spec, string sectionId)
        {
            return sectionId switch
            {
                "BoardGrid" => $"{spec.boardGrid.width}x{spec.boardGrid.height}",
                "ItemTray" => $"{spec.itemTray.columnCount}x{spec.itemTray.rowCount} slots={spec.itemTray.slotCount}",
                "CategoryTabs" => $"categories={spec.categoryTabs.categoryIds.Count}",
                "SelectedItemInfo" => "slot=SelectedItemInfo",
                "PlacementFeedback" => $"states={spec.placementFeedback.feedbackStates.Count}",
                "ActionButtons" => $"actions={spec.actionButtons.actionIds.Count}",
                "DataPanelDock" => $"slots={spec.dataPanelDock.bindingSlotNames.Count}",
                "BattleUiReuse" => $"channels={spec.battleUiReuse.recommendations.Count}",
                "DeveloperTuningPanel" => $"fields={spec.developerTuningPanel.fields.Count}",
                "PlayerHintMasking" => $"rules={spec.playerHintMasking.maskingRules.Count}",
                _ => string.Empty
            };
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

        private static string Csv(params string[] values)
        {
            return string.Join(",", values.Select(EscapeCsv));
        }

        private static string Escape(string value)
        {
            return (value ?? string.Empty)
                .Replace("|", "\\|")
                .Replace("\r", " ")
                .Replace("\n", " ");
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
