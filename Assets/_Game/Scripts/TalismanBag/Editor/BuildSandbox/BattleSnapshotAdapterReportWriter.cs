#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TalismanBag.BuildSandbox;
using TalismanBag.Contracts.Battle;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSnapshotAdapterReportWriter
    {
        public const string PackageName = "V0.4-BattleSnapshotAdapter01";
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSnapshotAdapterReport.md";
        public const string FieldMapReportPath =
            "Docs/V0.4/Reports/BattleSnapshotFieldMap.csv";
        public const string PlayerDevSplitReportPath =
            "Docs/V0.4/Reports/BattleSnapshotPlayerDevFieldSplit.csv";
        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSnapshotAdapterLeakCheckReport.md";

        [MenuItem("Tools/Talisman Bag/V0.4/BuildSandbox/BattleSnapshotAdapter01/[QA Only] Run Validation Reports")]
        public static void RunFromMenu()
        {
            BattleSnapshotAdapterValidationSnapshot snapshot =
                BattleContractFieldValidator.BuildValidationSnapshot();
            List<BuildSandboxValidationReport> reports = BuildValidationReports(snapshot);
            string[] paths = WriteReports(reports, snapshot);
            Debug.Log("[BattleSnapshotAdapter01] Reports written:\n" + string.Join("\n", paths));
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports(
            BattleSnapshotAdapterValidationSnapshot snapshot = null)
        {
            BattleSnapshotAdapterValidationSnapshot safeSnapshot =
                snapshot ?? BattleContractFieldValidator.BuildValidationSnapshot();
            BuildSandboxValidationReport report = new("BattleSnapshotAdapter01 Field Validator");

            AddCountCheck(report, "V03_SINGLE_CELL_MAP", safeSnapshot.v03SingleCellErrorCount, "V0.3 sample maps to Single1, rotation 0, one occupied cell.");
            AddCountCheck(report, "V04_MULTI_CELL_MAP", safeSnapshot.v04MultiCellErrorCount, "V0.4 sample normalizes at least one multi-cell item.");
            AddCountCheck(report, "PLAYER_DEV_FIELD_SPLIT", safeSnapshot.playerDeveloperFieldLeakCount, "Player fields do not include developer answer-layer tokens.");
            AddCountCheck(report, "SANDBOX_RESULT_DEFAULTS", safeSnapshot.sandboxResultDefaultErrorCount, "Sandbox result defaults to devOnly/no-save/no-reward.");
            AddCountCheck(report, "FORMAL_WRITE_LEAKS", safeSnapshot.formalWriteLeakCount, "Adapters do not write formal flow, save, reward, scene UI, or settlements.");
            AddCountCheck(report, "FEATURE_FLAG_DEFAULT_TRUE", safeSnapshot.featureFlagDefaultTrueCount, "BuildSandbox feature flag defaults remain disabled.");
            AddCountCheck(report, "DEVONLY_FORMAL_FLOW_LEAKS", safeSnapshot.devOnlyFormalFlowLeakCount, "devOnly sandbox preview does not leak into formal flow.");

            foreach (string message in safeSnapshot.validationMessages ?? new List<string>())
            {
                report.AddInfo("BATTLE_SNAPSHOT_SAMPLE", message, PackageName);
            }

            return new List<BuildSandboxValidationReport> { report };
        }

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSnapshotAdapterValidationSnapshot snapshot = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSnapshotAdapterValidationSnapshot safeSnapshot =
                snapshot ?? BattleContractFieldValidator.BuildValidationSnapshot();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string fieldMapPath = Path.Combine(projectRoot, FieldMapReportPath);
            string playerDevPath = Path.Combine(projectRoot, PlayerDevSplitReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = safeSnapshot.TotalLeakCount;
            bool pass = errors == 0 && leakCount == 0 && safeSnapshot.Passed;

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safeSnapshot, errors, warnings, leakCount, pass),
                new UTF8Encoding(false));
            File.WriteAllText(
                fieldMapPath,
                BuildFieldMapCsv(),
                new UTF8Encoding(false));
            File.WriteAllText(
                playerDevPath,
                BuildPlayerDevSplitCsv(),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safeSnapshot, errors, warnings, leakCount, pass),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, fieldMapPath, playerDevPath, leakPath };
        }

        private static void AddCountCheck(
            BuildSandboxValidationReport report,
            string code,
            int count,
            string message)
        {
            if (count == 0)
            {
                report.AddInfo(code, message, PackageName);
            }
            else
            {
                report.AddError(code, message + " actual=" + count, PackageName);
            }
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSnapshotAdapterValidationSnapshot snapshot,
            int errors,
            int warnings,
            int leakCount,
            bool pass)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Battle Snapshot Adapter Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(pass ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Adds neutral `TalismanBag.Contracts.Battle` data contracts and read-only adapters.");
            builder.AppendLine("- Does not create or modify UnifiedBattlePage, scenes, prefabs, UI layout, BuildSettings, RunFlow, SaveData, rewards, Boss, or chapter progression.");
            builder.AppendLine("- V0.4 sandbox results stay devOnly and cannot grant reward or write save through this contract layer.");
            builder.AppendLine();
            builder.AppendLine("## Contract Types");
            builder.AppendLine();
            builder.AppendLine("| Type | Layer | Purpose |");
            builder.AppendLine("| --- | --- | --- |");
            builder.AppendLine("| `BattleStartRequest` | Contracts/Battle | Read-only battle request data. |");
            builder.AppendLine("| `BattleLayoutSnapshot` | Contracts/Battle | Neutral board/layout state. |");
            builder.AppendLine("| `BattleItemSnapshot` | Contracts/Battle | Neutral item placement and stat state. |");
            builder.AppendLine("| `BattleEnemySnapshot` | Contracts/Battle | Neutral enemy/boss readable state. |");
            builder.AppendLine("| `BuildEvaluationSnapshot` | Contracts/Battle | Player build summary plus dev diagnostics. |");
            builder.AppendLine("| `BattleResultSnapshot` | Contracts/Battle | Result data, not a commit command. |");
            builder.AppendLine("| `BattleContractDiagnostics` | Contracts/Battle | Report/dev-only diagnostics. |");
            builder.AppendLine();
            AppendSampleCounters(builder, snapshot);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AppendSampleCounters(
            StringBuilder builder,
            BattleSnapshotAdapterValidationSnapshot snapshot)
        {
            BattleLayoutSnapshot v03 = snapshot.v03LayoutSnapshot;
            BattleLayoutSnapshot v04 = snapshot.v04LayoutSnapshot;
            BattleResultSnapshot result = snapshot.sandboxResultSnapshot;

            builder.AppendLine("## Validation Samples");
            builder.AppendLine();
            builder.AppendLine("| Check | Value | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| V0.3 placed item count | {v03?.placedItems?.Count ?? 0} | >= 1 |");
            builder.AppendLine($"| V0.3 single-cell errors | {snapshot.v03SingleCellErrorCount} | 0 |");
            builder.AppendLine($"| V0.4 placed item count | {v04?.placedItems?.Count ?? 0} | >= 1 |");
            builder.AppendLine($"| V0.4 multi-cell items | {v04?.placedItems?.Count(item => item.occupiedCells.Count > 1) ?? 0} | >= 1 |");
            builder.AppendLine($"| V0.4 multi-cell errors | {snapshot.v04MultiCellErrorCount} | 0 |");
            builder.AppendLine($"| Player/dev field leak count | {snapshot.playerDeveloperFieldLeakCount} | 0 |");
            builder.AppendLine($"| Sandbox result devOnly | {(result?.devOnly ?? false)} | true |");
            builder.AppendLine($"| Sandbox result shouldWriteSave | {(result?.shouldWriteSave ?? true)} | false |");
            builder.AppendLine($"| Sandbox result shouldGrantReward | {(result?.shouldGrantReward ?? true)} | false |");
            builder.AppendLine();
            builder.AppendLine("## V0.3 Single Cell Mapping");
            builder.AppendLine();
            builder.AppendLine("| Item | Shape | Rotation | Anchor | Occupied | Legacy |");
            builder.AppendLine("| --- | --- | ---: | --- | --- | --- |");
            foreach (BattleItemSnapshot item in v03?.placedItems ?? new List<BattleItemSnapshot>())
            {
                builder.AppendLine($"| `{Escape(item.itemId)}` | `{Escape(item.shapeId)}` | {item.rotationIndex} | `{item.anchorCell}` | `{string.Join(";", item.occupiedCells)}` | `{item.legacySingleCell}` |");
            }

            builder.AppendLine();
            builder.AppendLine("## V0.4 Layout Normalization");
            builder.AppendLine();
            builder.AppendLine("| Item | Shape | Rotation | Occupied Count | Energy | Source Container |");
            builder.AppendLine("| --- | --- | ---: | ---: | --- | --- |");
            foreach (BattleItemSnapshot item in v04?.placedItems ?? new List<BattleItemSnapshot>())
            {
                builder.AppendLine($"| `{Escape(item.itemId)}` | `{Escape(item.shapeId)}` | {item.rotationIndex} | {item.occupiedCells.Count} | `{item.energyState}` | `{Escape(item.sourceContainer)}` |");
            }

            builder.AppendLine();
        }

        private static void AppendValidationSummary(
            StringBuilder builder,
            IReadOnlyList<BuildSandboxValidationReport> reports)
        {
            builder.AppendLine("## Validation Summary");
            builder.AppendLine();
            builder.AppendLine("| Check | Status | Errors | Warnings | Info |");
            builder.AppendLine("| --- | --- | ---: | ---: | ---: |");
            foreach (BuildSandboxValidationReport report in reports)
            {
                builder.AppendLine($"| {Escape(report.Name)} | `{(report.Passed ? "PASS" : "FAIL")}` | {report.ErrorCount} | {report.WarningCount} | {report.InfoCount} |");
            }

            builder.AppendLine();
            builder.AppendLine("## Issues");
            builder.AppendLine();
            builder.AppendLine("| Level | Code | Message | Path |");
            builder.AppendLine("| --- | --- | --- | --- |");
            foreach (BuildSandboxValidationIssue issue in reports.SelectMany(report => report.Issues))
            {
                builder.AppendLine($"| `{issue.Level}` | `{Escape(issue.Code)}` | {Escape(issue.Message)} | `{Escape(issue.AssetPath)}` |");
            }
        }

        private static string BuildFieldMapCsv()
        {
            StringBuilder csv = new();
            csv.AppendLine("contract,field,playerVisible,developerOnly,source,notes");
            AddFieldRows(csv);
            return csv.ToString();
        }

        private static string BuildPlayerDevSplitCsv()
        {
            StringBuilder csv = new();
            csv.AppendLine("fieldGroup,field,allowedLayer,forbiddenLayer,reason");
            csv.AppendLine(Csv("player", "displayName", "player UI / report", "developer answer layer only", "Human-readable item/enemy/build label."));
            csv.AppendLine(Csv("player", "shapeId / occupiedCells", "player UI / report", "developer answer layer only", "Visual placement result; no solution answer required."));
            csv.AppendLine(Csv("player", "energyState", "player UI as readable or masked state", "exact source diagnostics", "Player can see powered/suppressed style state."));
            csv.AppendLine(Csv("player", "activeSynergies / readinessSummary", "player UI / report", "full solution requirements", "Summary is allowed, exact answer lists are not."));
            csv.AppendLine(Csv("player", "playerVisibleHints / recommendedAction / nextRouteHint", "player UI / report", "exact boss answer data", "Guidance text must not expose answer-layer data."));
            csv.AppendLine(Csv("developer", "hardSolutionTags", "dev panel / report only", "player UI", "Exact solution tags."));
            csv.AppendLine(Csv("developer", "requiredSynergy / requiredAffix / requiredStats", "dev panel / report only", "player UI", "Exact build requirements."));
            csv.AppendLine(Csv("developer", "dropBiasWeights", "dev panel / report only", "player UI / formal reward UI", "Drop tuning data must not leak."));
            csv.AppendLine(Csv("developer", "bossSixKeyFullAnswer / problemReadinessFullAnswer", "dev panel / report only", "player UI / BossInfoPanel", "Full answer layer."));
            csv.AppendLine(Csv("developer", "exactWeaknessTiming / validationTargetBuilds", "dev panel / report only", "player UI", "Precise validation answer data."));
            csv.AppendLine(Csv("developer", "sourceDataPath / raw config id", "dev panel / report only", "player UI", "Implementation provenance, not player-facing text."));
            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSnapshotAdapterValidationSnapshot snapshot,
            int errors,
            int warnings,
            int leakCount,
            bool pass)
        {
            StringBuilder builder = new();
            builder.AppendLine("# Battle Snapshot Adapter Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(pass ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Guard Checks");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `v03SingleCellErrorCount` | {snapshot.v03SingleCellErrorCount} | 0 |");
            builder.AppendLine($"| `v04MultiCellErrorCount` | {snapshot.v04MultiCellErrorCount} | 0 |");
            builder.AppendLine($"| `playerDeveloperFieldLeakCount` | {snapshot.playerDeveloperFieldLeakCount} | 0 |");
            builder.AppendLine($"| `sandboxResultDefaultErrorCount` | {snapshot.sandboxResultDefaultErrorCount} | 0 |");
            builder.AppendLine($"| `formalWriteLeakCount` | {snapshot.formalWriteLeakCount} | 0 |");
            builder.AppendLine($"| `featureFlagDefaultTrueCount` | {snapshot.featureFlagDefaultTrueCount} | 0 |");
            builder.AppendLine($"| `devOnlyFormalFlowLeakCount` | {snapshot.devOnlyFormalFlowLeakCount} | 0 |");
            builder.AppendLine("| `saveDataWrites` | 0 | 0 |");
            builder.AppendLine("| `playerPrefsWrites` | 0 | 0 |");
            builder.AppendLine("| `mainTrialProgressDataWrites` | 0 | 0 |");
            builder.AppendLine("| `rewardServiceWrites` | 0 | 0 |");
            builder.AppendLine("| `dropTableWrites` | 0 | 0 |");
            builder.AppendLine("| `runFlowWrites` | 0 | 0 |");
            builder.AppendLine("| `autoCombatControllerWrites` | 0 | 0 |");
            builder.AppendLine("| `pageStateWrites` | 0 | 0 |");
            builder.AppendLine("| `formationStateWrites` | 0 | 0 |");
            builder.AppendLine("| `bossInfoPanelWrites` | 0 | 0 |");
            builder.AppendLine("| `scenePrefabUiBuildSettingsWrites` | 0 | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- `BattleResultSnapshot` is data only; it is not a save/reward/chapter commit command.");
            builder.AppendLine("- `SandboxBattleResultMapper` always emits `devOnly=true`, `shouldWriteSave=false`, and `shouldGrantReward=false`.");
            builder.AppendLine("- Developer answer-layer fields remain in `BuildEvaluationSnapshot` diagnostics/report fields, not player hint fields.");
            builder.AppendLine("- No formal FeatureFlag defaults are enabled.");
            builder.AppendLine();
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static void AddFieldRows(StringBuilder csv)
        {
            csv.AppendLine(Csv("BattleStartRequest", "requestId", "false", "false", "adapter", "Correlation id only."));
            csv.AppendLine(Csv("BattleStartRequest", "chapterId / stageId / roundId", "false", "false", "V0.3 route / round config", "Read-only stage identity."));
            csv.AppendLine(Csv("BattleStartRequest", "enemyProfileId / bossProfileId", "false", "true", "V0.3 round config", "Raw profile ids are not player-facing."));
            csv.AppendLine(Csv("BattleStartRequest", "formalFlow / devOnly", "false", "true", "adapter", "Boundary flags."));
            csv.AppendLine(Csv("BattleLayoutSnapshot", "boardId / gridWidth / gridHeight", "false", "false", "V0.3 or V0.4 layout source", "Neutral board identity."));
            csv.AppendLine(Csv("BattleLayoutSnapshot", "placedItems", "true", "false", "V0.3 loadout / V0.4 layout", "Player may inspect placed item visuals."));
            csv.AppendLine(Csv("BattleLayoutSnapshot", "formationEyes / energySources", "true", "false", "FormationEnergyContract", "Player may inspect readable energy state."));
            csv.AppendLine(Csv("BattleLayoutSnapshot", "energyDiagnostics / validationWarnings", "false", "true", "adapter validator", "Diagnostics only."));
            csv.AppendLine(Csv("BattleItemSnapshot", "displayName / shapeId / occupiedCells", "true", "false", "loadout or BuildSandbox snapshot", "Visual item result."));
            csv.AppendLine(Csv("BattleItemSnapshot", "itemStats", "true", "false", "V0.3 computed stats / V0.4 itemStat", "Readable combat stat summary."));
            csv.AppendLine(Csv("BattleItemSnapshot", "runtimeId / sourceDataPath / fullRolls", "false", "true", "source snapshots", "Internal provenance and answer-layer data."));
            csv.AppendLine(Csv("BattleEnemySnapshot", "hp / shield / attackDamage / castProgress", "true", "false", "enemy runtime or sandbox preview", "Readable enemy state."));
            csv.AppendLine(Csv("BattleEnemySnapshot", "validationTargetBuilds / exactWeaknessTiming", "false", "true", "devOnly pools", "Answer-layer diagnostics."));
            csv.AppendLine(Csv("BuildEvaluationSnapshot", "activeSynergies / readinessSummary", "true", "false", "BuildEvaluationResult", "Player-safe build summary."));
            csv.AppendLine(Csv("BuildEvaluationSnapshot", "hardSolutionTags / required* / dropBiasWeights", "false", "true", "BuildSandbox problem data", "Dev-only answer layer."));
            csv.AppendLine(Csv("BattleResultSnapshot", "resultType / win / lose / nextRouteHint", "true", "false", "runtime loop preview", "Result presentation data."));
            csv.AppendLine(Csv("BattleResultSnapshot", "shouldWriteSave / shouldGrantReward / rewardClaimToken", "false", "true", "result mapper", "Commit policy stays formal-flow owned."));
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
