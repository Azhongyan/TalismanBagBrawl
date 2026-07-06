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
    public static class BattleSandboxRuntimeLoopReportWriter
    {
        public const string MainReportPath =
            "Docs/V0.4/Reports/BattleSandboxRuntimeLoopReport.md";

        public const string RowReportPath =
            "Docs/V0.4/Reports/BattleSandboxRuntimeLoopRows.csv";

        public const string LeakCheckReportPath =
            "Docs/V0.4/Reports/BattleSandboxRuntimeLoopLeakCheckReport.md";

        public static string[] WriteReports(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxRuntimeLoopPreview preview = null)
        {
            IReadOnlyList<BuildSandboxValidationReport> safeReports =
                reports ?? Array.Empty<BuildSandboxValidationReport>();
            BattleSandboxRuntimeLoopPreview safePreview =
                preview ?? BattleSandboxRuntimeLoopValidator.BuildDefaultPreview();

            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string mainPath = Path.Combine(projectRoot, MainReportPath);
            string rowPath = Path.Combine(projectRoot, RowReportPath);
            string leakPath = Path.Combine(projectRoot, LeakCheckReportPath);
            Directory.CreateDirectory(Path.GetDirectoryName(mainPath) ?? projectRoot);

            int errors = safeReports.Sum(report => report.ErrorCount);
            int warnings = safeReports.Sum(report => report.WarningCount);
            int leakCount = CountLeaks(safePreview);

            File.WriteAllText(
                mainPath,
                BuildMainReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));
            File.WriteAllText(
                rowPath,
                BuildRowsCsv(safePreview),
                new UTF8Encoding(false));
            File.WriteAllText(
                leakPath,
                BuildLeakCheckReport(safeReports, safePreview, errors, warnings, leakCount),
                new UTF8Encoding(false));

            AssetDatabase.Refresh();
            return new[] { mainPath, rowPath, leakPath };
        }

        private static string BuildMainReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxRuntimeLoopPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Runtime Loop Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxRuntimeLoopValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine($"Leak Count: `{leakCount}`");
            builder.AppendLine();
            builder.AppendLine("## Scope");
            builder.AppendLine();
            builder.AppendLine("- Runs only inside `Scene_TalismanBag_V04_BattleSandboxPreview` runtime surfaces.");
            builder.AppendLine("- Reads `CombatKernelAdapter`, current V04 BuildSandbox board snapshots, BuildCombatPreview, and BuildSandbox ItemStat.");
            builder.AppendLine("- Refreshes existing HUD text/fill, Boss cast bar text/fill, combat log, and runtime floating text.");
            builder.AppendLine("- Emits sandbox-only victory/defeat result prompts without formal settlement, RunFlow, SaveData, Reward, or Chapter progress.");
            builder.AppendLine("- Reuses DevChapterBalanceRun devOnly 3-10/4-10 test enemy metadata for scenario switching.");
            builder.AppendLine("- Does not author or reorder V04 UI layout.");
            builder.AppendLine();
            builder.AppendLine("## Counters");
            builder.AppendLine();
            builder.AppendLine($"- rows: `{preview?.rows?.Count ?? 0}`");
            builder.AppendLine($"- mana rows: `{preview?.ManaRowCount ?? 0}`");
            builder.AppendLine($"- cooldown rows: `{preview?.CooldownRowCount ?? 0}`");
            builder.AppendLine($"- item trigger rows: `{preview?.ItemTriggerRowCount ?? 0}`");
            builder.AppendLine($"- enemy HP rows: `{preview?.EnemyHpRowCount ?? 0}`");
            builder.AppendLine($"- enemy shield update rows: `{preview?.EnemyShieldRowCount ?? 0}`");
            builder.AppendLine($"- player HP rows: `{preview?.PlayerHpRowCount ?? 0}`");
            builder.AppendLine($"- player shield rows: `{preview?.PlayerShieldRowCount ?? 0}`");
            builder.AppendLine($"- Boss cast rows: `{preview?.BossCastRowCount ?? 0}`");
            builder.AppendLine($"- enemy attack rows: `{preview?.EnemyAttackRowCount ?? 0}`");
            builder.AppendLine($"- enemy attack timer advance rows: `{preview?.EnemyAttackTimerAdvanceRowCount ?? 0}`");
            builder.AppendLine($"- devOnly profile attack damage rows: `{preview?.DevOnlyProfileAttackDamageRowCount ?? 0}`");
            builder.AppendLine($"- shield-first player damage rows: `{preview?.ShieldFirstPlayerDamageRowCount ?? 0}`");
            builder.AppendLine($"- combat log rows: `{preview?.CombatLogRowCount ?? 0}`");
            builder.AppendLine($"- floating text rows: `{preview?.FloatingTextRowCount ?? 0}`");
            builder.AppendLine($"- sandbox result rows: `{preview?.SandboxResultRowCount ?? 0}`");
            builder.AppendLine($"- sandbox victory rows: `{preview?.SandboxVictoryResultRowCount ?? 0}`");
            builder.AppendLine($"- sandbox defeat rows: `{preview?.SandboxDefeatResultRowCount ?? 0}`");
            builder.AppendLine($"- current board placed items: `{preview?.currentBoardPlacedItemCount ?? 0}`");
            builder.AppendLine($"- default layout used as combat input: `{preview?.runtimeUsesDefaultLayoutAsCombatInput == true}`");
            builder.AppendLine($"- player item enemy HP damage total: `{preview?.playerItemEnemyHpDamageTotal ?? 0}`");
            builder.AppendLine($"- selected dev enemy: `{Escape(preview?.selectedDevEnemyLabel)} {Escape(preview?.selectedDevEnemyDisplayNameChinese)}`");
            builder.AppendLine($"- selected dev enemy attack: `{preview?.selectedDevEnemyAttackDamage ?? 0}` every `{preview?.selectedDevEnemyAttackIntervalSeconds ?? 0f:0.###}`s from `{Escape(preview?.selectedDevEnemyAttackSourcePath)}`");
            builder.AppendLine($"- source CombatKernelAdapter rows: `{preview?.sourceCombatKernelAdapterRowCount ?? 0}`");
            builder.AppendLine($"- source BuildCombatPreview rows: `{preview?.sourceBuildCombatPreviewRowCount ?? 0}`");
            builder.AppendLine($"- source ItemStat profiles: `{preview?.sourceItemStatProfileCount ?? 0}`");
            builder.AppendLine($"- formal leak count: `{preview?.FormalLeakCount ?? 1}`");
            builder.AppendLine($"- settlement leak count: `{preview?.SettlementLeakCount ?? 1}`");
            builder.AppendLine($"- UI layout write count: `{preview?.UiLayoutWriteCount ?? 1}`");
            builder.AppendLine();
            AppendRows(builder, preview);
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static string BuildRowsCsv(BattleSandboxRuntimeLoopPreview preview)
        {
            StringBuilder csv = new();
            csv.AppendLine("rowId,rowKind,itemId,statProfileId,itemEffectKey,itemEffectFamilyChinese,itemEffectRoleChinese,elapsedSeconds,manaBefore,manaAfter,manaDelta,cooldownSeconds,enemyHpBefore,enemyHpAfter,enemyShieldBefore,enemyShieldAfter,playerHpBefore,playerHpAfter,playerShieldBefore,playerShieldAfter,bossCastRemainingSeconds,bossCastFillAmount,enemyAttackTimerBeforeSeconds,enemyAttackTimerAfterSeconds,enemyAttackDamage,playerShieldDamageAbsorbed,playerHpDamageApplied,enemyAttackTimerAdvanced,enemyAttackFromDevOnlyProfile,playerShieldDamageResolvedFirst,stateLineChinese,castSkillLineChinese,combatLogLineChinese,floatingTextChinese,sourceDataPath,developerDataPanelFieldKey,formalFlowLeak,playerSideAnswerLeak,uiLayoutWrite,hasVictorySettlement,hasDefeatSettlement,hasSandboxVictoryResult,hasSandboxDefeatResult,locksRuntimeLoop,resultTitleChinese,restartHintChinese");
            foreach (BattleSandboxRuntimeLoopRow row in BattleSandboxRuntimeLoopValidator.Rows(preview))
            {
                csv.AppendLine(Csv(
                    row.rowId,
                    row.rowKind,
                    row.itemId,
                    row.statProfileId,
                    row.itemEffectKey,
                    row.itemEffectFamilyChinese,
                    row.itemEffectRoleChinese,
                    row.elapsedSeconds.ToString("0.###"),
                    row.manaBefore.ToString(),
                    row.manaAfter.ToString(),
                    row.manaDelta.ToString(),
                    row.cooldownSeconds.ToString("0.###"),
                    row.enemyHpBefore.ToString(),
                    row.enemyHpAfter.ToString(),
                    row.enemyShieldBefore.ToString(),
                    row.enemyShieldAfter.ToString(),
                    row.playerHpBefore.ToString(),
                    row.playerHpAfter.ToString(),
                    row.playerShieldBefore.ToString(),
                    row.playerShieldAfter.ToString(),
                    row.bossCastRemainingSeconds.ToString("0.###"),
                    row.bossCastFillAmount.ToString("0.###"),
                    row.enemyAttackTimerBeforeSeconds.ToString("0.###"),
                    row.enemyAttackTimerAfterSeconds.ToString("0.###"),
                    row.enemyAttackDamage.ToString(),
                    row.playerShieldDamageAbsorbed.ToString(),
                    row.playerHpDamageApplied.ToString(),
                    row.enemyAttackTimerAdvanced.ToString(),
                    row.enemyAttackFromDevOnlyProfile.ToString(),
                    row.playerShieldDamageResolvedFirst.ToString(),
                    row.stateLineChinese,
                    row.castSkillLineChinese,
                    row.combatLogLineChinese,
                    row.floatingTextChinese,
                    row.sourceDataPath,
                    row.developerDataPanelFieldKey,
                    row.formalFlowLeak.ToString(),
                    row.playerSideAnswerLeak.ToString(),
                    row.uiLayoutWrite.ToString(),
                    row.hasVictorySettlement.ToString(),
                    row.hasDefeatSettlement.ToString(),
                    row.hasSandboxVictoryResult.ToString(),
                    row.hasSandboxDefeatResult.ToString(),
                    row.locksRuntimeLoop.ToString(),
                    row.resultTitleChinese,
                    row.restartHintChinese));
            }

            return csv.ToString();
        }

        private static string BuildLeakCheckReport(
            IReadOnlyList<BuildSandboxValidationReport> reports,
            BattleSandboxRuntimeLoopPreview preview,
            int errors,
            int warnings,
            int leakCount)
        {
            StringBuilder builder = new();
            builder.AppendLine("# BattleSandbox Runtime Loop Leak Check Report");
            builder.AppendLine();
            builder.AppendLine($"Package: `{BattleSandboxRuntimeLoopValidator.PackageName}`");
            builder.AppendLine($"Generated: `{DateTime.Now:yyyy-MM-dd HH:mm:ss}`");
            builder.AppendLine($"Status: `{(errors == 0 && leakCount == 0 ? "PASS" : "FAIL")}`");
            builder.AppendLine($"Errors: `{errors}`");
            builder.AppendLine($"Warnings: `{warnings}`");
            builder.AppendLine();
            builder.AppendLine("## Leak Counters");
            builder.AppendLine();
            builder.AppendLine("| Check | Count | Expected |");
            builder.AppendLine("| --- | ---: | ---: |");
            builder.AppendLine($"| `featureFlagDefaultTrue` | {preview?.FeatureFlagDefaultTrueCount ?? 1} | 0 |");
            builder.AppendLine($"| `formalFlowLeaks` | {preview?.FormalLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `settlementLeaks` | {preview?.SettlementLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `playerSideAnswerLeaks` | {preview?.PlayerSideAnswerLeakCount ?? 1} | 0 |");
            builder.AppendLine($"| `uiLayoutWrites` | {preview?.UiLayoutWriteCount ?? 1} | 0 |");
            builder.AppendLine($"| `runFlowWrites` | {(preview?.writesFormalFlow == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `saveWrites` | {(preview?.writesFormalSaveData == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `rewardGrants` | {(preview?.grantsFormalReward == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `chapterAdvances` | {(preview?.advancesChapter == true ? 1 : 0)} | 0 |");
            builder.AppendLine($"| `totalLeaks` | {leakCount} | 0 |");
            builder.AppendLine();
            builder.AppendLine("## Scope Confirmation");
            builder.AppendLine();
            builder.AppendLine("- Runtime loop stops on a sandbox-only victory/defeat result row and keeps formal settlement leak count at zero.");
            builder.AppendLine("- Runtime loop updates existing text/fill/floating surfaces only.");
            builder.AppendLine("- Runtime loop does not call formal combat settlement, saves, rewards, chapter, or RunFlow APIs.");
            AppendValidationSummary(builder, reports);
            return builder.ToString();
        }

        private static int CountLeaks(BattleSandboxRuntimeLoopPreview preview)
        {
            return (preview?.FeatureFlagDefaultTrueCount ?? 1)
                + (preview?.FormalLeakCount ?? 1)
                + (preview?.SettlementLeakCount ?? 1)
                + (preview?.PlayerSideAnswerLeakCount ?? 1)
                + (preview?.UiLayoutWriteCount ?? 1);
        }

        private static void AppendRows(
            StringBuilder builder,
            BattleSandboxRuntimeLoopPreview preview)
        {
            builder.AppendLine("## Runtime Loop Rows");
            builder.AppendLine();
            builder.AppendLine("| Row | Kind | Item | Mana | Enemy | Player | Cast | Attack | Floating | Source |");
            builder.AppendLine("| --- | --- | --- | ---: | --- | --- | ---: | --- | --- | --- |");
            foreach (BattleSandboxRuntimeLoopRow row in BattleSandboxRuntimeLoopValidator.Rows(preview))
            {
                builder.AppendLine(
                    $"| `{Escape(row.rowId)}` | `{Escape(row.rowKind)}` | `{Escape(row.itemId)}` | {row.manaAfter} | {row.enemyHpAfter}/{preview?.enemyMaxHp ?? 0} shield {row.enemyShieldAfter} | {row.playerHpAfter}/{preview?.playerMaxHp ?? 0} shield {row.playerShieldAfter} | {row.bossCastRemainingSeconds:0.0}s | dmg {row.enemyAttackDamage}, block {row.playerShieldDamageAbsorbed}, hp {row.playerHpDamageApplied} | {Escape(row.floatingTextChinese)} | `{Escape(row.sourceDataPath)}` |");
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
