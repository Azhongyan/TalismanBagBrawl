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
    public static class FormationEnergyContractValidator
    {
        public const string PackageName = FormationEnergyContractPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/FormationEnergyContract01/[QA Only] Run Formation Energy Contract";
        public static void RunMenu()
        {
            Run(throwOnFailure: false);
        }

        public static bool Run(bool throwOnFailure)
        {
            List<BuildSandboxValidationReport> reports = BuildValidationReports();
            FormationEnergyContractPreview preview = BuildDefaultPreview();
            string[] reportPaths = FormationEnergyContractReportWriter.WriteReports(reports, preview);
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
                $"[BuildSandbox-FormationEnergyContract01] completed errors={errors}, warnings={warnings}, reports={string.Join(", ", reportPaths)}");

            if (errors > 0 && throwOnFailure)
            {
                throw new InvalidOperationException(
                    $"BuildSandbox FormationEnergyContract01 failed with {errors} error(s). See {string.Join(", ", reportPaths)}");
            }

            return errors == 0;
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                Validate()
            };
        }

        public static FormationEnergyContractPreview BuildDefaultPreview()
        {
            BattleSandboxBuildCombatPreview combatPreview =
                BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreview();
            return FormationEnergyContractResolver.Apply(combatPreview?.context?.layoutSnapshot);
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Formation Energy Contract 01");
            FormationEnergyContractPreview preview = BuildDefaultPreview();
            ValidateIsolation(report, preview);
            ValidateContractState(report, preview);
            ValidateRecordedDiagnostics(report, preview);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            FormationEnergyContractPreview preview)
        {
            if (preview == null)
            {
                report.AddError("FORMATION_ENERGY_PREVIEW_NULL", "Formation energy contract preview was not created.", PackageName);
                return;
            }

            if (!preview.devOnly || preview.isEnabled || preview.ScopeLeakCount != 0)
            {
                report.AddError(
                    "FORMATION_ENERGY_SCOPE_LEAK",
                    $"Contract must stay devOnly=true, isEnabled=false, and scopeLeak=0. actualScopeLeak={preview.ScopeLeakCount}.",
                    nameof(FormationEnergyContractPreview));
            }

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "FORMATION_ENERGY_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }
        }

        private static void ValidateContractState(
            BuildSandboxValidationReport report,
            FormationEnergyContractPreview preview)
        {
            IReadOnlyList<BuildSandboxPlacedItemSnapshot> items =
                preview?.battleLayoutSnapshot?.placedItems
                ?? (IReadOnlyList<BuildSandboxPlacedItemSnapshot>)Array.Empty<BuildSandboxPlacedItemSnapshot>();

            if (items.Count == 0)
            {
                report.AddError("FORMATION_ENERGY_ITEM_ROWS_MISSING", "Default preview must include placed item snapshots.", PackageName);
                return;
            }

            if (preview.EnergyStoneProviderCount < 1)
            {
                report.AddError(
                    "FORMATION_ENERGY_STONE_PROVIDER_MISSING",
                    "Default preview must contain at least one connected spirit/energy stone Powered provider.",
                    nameof(EnergyStoneConfig));
            }

            foreach (BuildSandboxPlacedItemSnapshot item in items)
            {
                if (item == null)
                {
                    continue;
                }

                bool derivedPowered = item.energyState == EnergyState.Powered;
                if (item.isPowered != derivedPowered)
                {
                    report.AddError(
                        "FORMATION_ENERGY_ISPOWERED_NOT_DERIVED",
                        $"isPowered must equal energyState==Powered. itemId={item.itemId}, state={item.energyState}, isPowered={item.isPowered}.",
                        nameof(BuildSandboxPlacedItemSnapshot));
                }

                if (!FormationEnergyContractResolver.IsEnergyStoneItem(item)
                    && string.Equals(item.energySourceId, item.itemId, StringComparison.Ordinal)
                    && item.energyState == EnergyState.Powered)
                {
                    report.AddError(
                        "FORMATION_ENERGY_NONSTONE_SELF_PROVIDER",
                        $"Non-stone item must not power itself. itemId={item.itemId}.",
                        nameof(BuildSandboxPlacedItemSnapshot));
                }

                if (item.energyState == EnergyState.WeakPulse && item.isPowered)
                {
                    report.AddError(
                        "FORMATION_ENERGY_WEAKPULSE_POWERED",
                        $"WeakPulse must not set isPowered. itemId={item.itemId}.",
                        nameof(BuildSandboxPlacedItemSnapshot));
                }
            }

            report.AddInfo(
                "FORMATION_ENERGY_COUNTS",
                $"providers={preview.EnergyStoneProviderCount}, powered={preview.PoweredItemCount}, weakPulse={preview.WeakPulseItemCount}, suppressed={preview.SuppressedItemCount}, diagnostics={preview.diagnostics.Count}.",
                nameof(FormationEnergyContractPreview));
        }

        private static void ValidateRecordedDiagnostics(
            BuildSandboxValidationReport report,
            FormationEnergyContractPreview preview)
        {
            if (preview == null)
            {
                return;
            }

            if (preview.EyeCellOccupiedCount > 0)
            {
                report.AddError(
                    "FORMATION_ENERGY_EYE_CELL_OCCUPIED",
                    $"The default preview must keep the eye cell empty. occupiedCount={preview.EyeCellOccupiedCount}.",
                    nameof(BackpackLayoutConfig));
            }

            if (preview.ForbiddenProviderCandidateCount > 0
                || preview.TagProviderViolationCount > 0
                || preview.LegacyProviderViolationCount > 0)
            {
                report.AddWarning(
                    "FORMATION_ENERGY_LEGACY_VIOLATIONS_RECORDED",
                    $"Recorded legacy conflict diagnostics: forbiddenProvider={preview.ForbiddenProviderCandidateCount}, tagProvider={preview.TagProviderViolationCount}, legacyProvider={preview.LegacyProviderViolationCount}. These are report-visible migration notes, not active providers.",
                    nameof(FormationEnergyDiagnosticRow));
            }

            foreach (FormationEnergyDiagnosticRow diagnostic in preview.diagnostics ?? new List<FormationEnergyDiagnosticRow>())
            {
                if (diagnostic == null)
                {
                    continue;
                }

                report.AddInfo(
                    "FORMATION_ENERGY_DIAGNOSTIC_" + diagnostic.code,
                    $"{diagnostic.itemId}: {diagnostic.detail}",
                    diagnostic.sourceDataPath);
            }
        }
    }

    public static class FormationEnergyContractReportWriter
    {
        private static readonly Encoding Utf8WithBom = new UTF8Encoding(true);

        public const string MainReportPath =
            "Docs/V0.4/Reports/FormationEnergyContractReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/FormationEnergyContractRows.csv";

        public const string DiagnosticReportPath =
            "Docs/V0.4/Reports/FormationEnergyContractDiagnostics.csv";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            FormationEnergyContractPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            FormationEnergyContractPreview safePreview =
                preview ?? FormationEnergyContractValidator.BuildDefaultPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string diagnosticPath = Path.Combine(projectRoot, DiagnosticReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            WriteUtf8BomText(mainPath, BuildMainReport(safeReports, safePreview));
            WriteUtf8BomText(rowPath, BuildRowsCsv(safePreview));
            WriteUtf8BomText(diagnosticPath, BuildDiagnosticsCsv(safePreview));

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, diagnosticPath };
        }

        private static void WriteUtf8BomText(string path, string contents)
        {
            byte[] preamble = Utf8WithBom.GetPreamble();
            byte[] body = Utf8WithBom.GetBytes(contents ?? string.Empty);
            File.WriteAllBytes(path, preamble.Concat(body).ToArray());
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            FormationEnergyContractPreview preview)
        {
            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            StringBuilder builder = new();
            builder.AppendLine("# Formation Energy Contract Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{FormationEnergyContractPreview.PackageName}`");
            builder.AppendLine($"Guard Pass: `{preview?.guardPass ?? "GUARD_PASS_FORMATION_ENERGY_CONTRACT01"}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && preview?.ScopeLeakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Recorded Contract Diagnostics: `{preview?.ContractViolationCount ?? 0}`");
            builder.AppendLine();
            builder.AppendLine("## Locked Contract");
            builder.AppendLine();
            builder.AppendLine("- Eye / formation core is layout state, not a formal energy source.");
            builder.AppendLine("- Eye only grants `WeakPulse`; `WeakPulse` must not set `isPowered=true`.");
            builder.AppendLine("- Only spirit stone / energy stone family items can be formal `Powered` providers.");
            builder.AppendLine("- Energy incense, furnace/core, peach-wood, seal, and provider-like tags are diagnostics only.");
            builder.AppendLine("- `isPowered` is derived from `energyState == Powered`.");
            builder.AppendLine("- Scope stays BuildSandbox devOnly; no formal flow, chapter, reward, save, or scene layout writes.");
            builder.AppendLine();
            builder.AppendLine("## Code Survey");
            builder.AppendLine();
            builder.AppendLine("| Area | Finding | Contract Response |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine("| `BuildSandboxPlacedItemSnapshot` | Existing snapshot already carried placement, tags, `isPowered`, source id, formation core fields, and item stat. | Added `energyState`, eye connection, formal source, weak-pulse, provider, and diagnostic fields; `isPowered` is derived. |");
            builder.AppendLine("| `FormationCorePowerRange` | Legacy resolver treated energy incense, furnace/core, and provider tags as sources. | Resolver now delegates to `FormationEnergyContractResolver`; old report path remains compatibility-only. |");
            builder.AppendLine("| `BattleSandboxCombatKernelAdapter` | Eye cross sample was previously `FullPowered`. | Eye cross / upgraded eye area now resolves to `WeakPowered`; stone range remains full power. |");
            builder.AppendLine("| `BattleSandboxRuntimeLoop` | Runtime bonuses and mana loop still read `isPowered` and non-stone mana stats. | Runtime checks now read `energyState`; mana generation only comes from Powered energy stones. |");
            builder.AppendLine("| `BuildGridInteractionPreviewController` | It builds current layout snapshots and calls preview energy links. | No UI/scene changes; current snapshot energy links now resolve through the new contract. |");
            builder.AppendLine("| `Reports/FormationCorePowerRange*` | Rows previously listed incense/core as providers. | Writer copy updated; generated rows now mark only energy stones as providers. |");
            builder.AppendLine();
            builder.AppendLine("## Similarities With V0.4");
            builder.AppendLine();
            builder.AppendLine("- Reuses BuildSandbox snapshot, preview, report, and validator infrastructure.");
            builder.AppendLine("- Reuses existing `spirit_stone_basic` / `spirit_stone` identity as the legal provider family.");
            builder.AppendLine("- Keeps item stat and battle preview as devOnly data surfaces.");
            builder.AppendLine("- Keeps FormationCorePowerRange feedback path available for existing reports.");
            builder.AppendLine();
            builder.AppendLine("## Active Counters");
            builder.AppendLine();
            builder.AppendLine("| Counter | Value |");
            builder.AppendLine("| --- | ---: |");
            builder.AppendLine($"| energyStoneProviderCount | {preview?.EnergyStoneProviderCount ?? 0} |");
            builder.AppendLine($"| poweredItemCount | {preview?.PoweredItemCount ?? 0} |");
            builder.AppendLine($"| weakPulseItemCount | {preview?.WeakPulseItemCount ?? 0} |");
            builder.AppendLine($"| suppressedItemCount | {preview?.SuppressedItemCount ?? 0} |");
            builder.AppendLine($"| forbiddenProviderCandidateCount | {preview?.ForbiddenProviderCandidateCount ?? 0} |");
            builder.AppendLine($"| tagProviderViolationCount | {preview?.TagProviderViolationCount ?? 0} |");
            builder.AppendLine($"| legacyProviderViolationCount | {preview?.LegacyProviderViolationCount ?? 0} |");
            builder.AppendLine($"| eyeCellOccupiedCount | {preview?.EyeCellOccupiedCount ?? 0} |");
            builder.AppendLine($"| scopeLeakCount | {preview?.ScopeLeakCount ?? 1} |");
            builder.AppendLine();
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(FormationEnergyContractPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("itemId,shapeId,energyState,isPowered,energySourceId,connectedEyeId,formalEnergySourceItemId,isEnergyStoneSource,isInBasePulseRange,isEyeAdjacent,touchesEyeCell,hasEnergyRoleViolation,powerRangeRadius,occupiedCells,powerRangeCells,powerConnectionState,energyStateReason,playerFeedbackChinese,devOnly,isEnabled");
            foreach (FormationEnergyContractRow row in preview?.rows ?? new List<FormationEnergyContractRow>())
            {
                csv.AppendLine(Csv(
                    row.itemId,
                    row.shapeId,
                    row.energyState.ToString(),
                    row.isPowered.ToString(),
                    row.energySourceId,
                    row.connectedEyeId,
                    row.formalEnergySourceItemId,
                    row.isEnergyStoneSource.ToString(),
                    row.isInBasePulseRange.ToString(),
                    row.isEyeAdjacent.ToString(),
                    row.touchesEyeCell.ToString(),
                    row.hasEnergyRoleViolation.ToString(),
                    row.powerRangeRadius.ToString(),
                    FormatCells(row.occupiedCells),
                    FormatCells(row.powerRangeCells),
                    row.powerConnectionState,
                    row.energyStateReason,
                    row.playerFeedbackChinese,
                    row.devOnly.ToString(),
                    row.isEnabled.ToString()));
            }

            return csv.ToString();
        }

        private static string BuildDiagnosticsCsv(FormationEnergyContractPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("code,severity,itemId,energyState,isPowered,isViolation,detail,sourceDataPath,devOnly,isEnabled");
            foreach (FormationEnergyDiagnosticRow row in preview?.diagnostics ?? new List<FormationEnergyDiagnosticRow>())
            {
                csv.AppendLine(Csv(
                    row.code,
                    row.severity,
                    row.itemId,
                    row.energyState.ToString(),
                    row.isPowered.ToString(),
                    row.isViolation.ToString(),
                    row.detail,
                    row.sourceDataPath,
                    row.devOnly.ToString(),
                    row.isEnabled.ToString()));
            }

            return csv.ToString();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports ?? Array.Empty<BuildSandboxValidationReport>())
            {
                builder.AppendLine(
                    $"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in (reports ?? Array.Empty<BuildSandboxValidationReport>()).SelectMany(report => report.Issues))
            {
                builder.AppendLine(
                    $"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static string FormatCells(IEnumerable<ItemShapeCell> cells)
        {
            return string.Join(";", (cells ?? Enumerable.Empty<ItemShapeCell>()).Select(cell => cell.ToString()));
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
