#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxPlayableLoopValidator
    {
        public const string PackageName = BattleSandboxPlayableLoop.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxPlayableLoop01/[QA Only] Run Playable Loop";

        private const string RuntimeLoopSourcePath =
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/BattleSandboxRuntimeLoop.cs";

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "hardSolutionTags",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "DropBias",
            "dropBias",
            "bossSixKeyFullAnswer",
            "Boss六钥匙",
            "Boss 六钥匙",
            "previewWeight",
            "keyRequirements"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            BattleSandboxPlayableLoopSnapshot snapshot = BuildDefaultSnapshot();
            List<BuildSandboxValidationReport> reports =
                BattleSandboxRuntimeLoopValidator.BuildValidationReports();
            reports.Add(Validate(snapshot));
            return reports;
        }

        public static BattleSandboxPlayableLoopSnapshot BuildDefaultSnapshot()
        {
            BattleSandboxRuntimeLoopPreview runtimePreview =
                BattleSandboxRuntimeLoopValidator.BuildDefaultPreview();
            BuildGridInteractionSceneBindingSnapshot sceneSnapshot =
                BuildGridInteractionPreviewValidator.BuildSceneBindingSnapshot();
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios =
                BattleSandboxRuntimeLoopPreviewBuilder.BuildDevEnemyScenarios();
            BuildSandboxLayoutSnapshot defaultLayout =
                BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot();

            bool restartRetainsBuild = ScanRuntimeLoopSource(
                "RestartLoop",
                "StartLoop()",
                "BuildCurrentLayoutSnapshot");
            bool restartResetsRuntime = ScanRuntimeLoopSource(
                "RestartLoop",
                "activePreview = null",
                "rowTimer = 0f",
                "rowIndex = 0",
                "resultLocked = false");
            bool supportsTargetSwitch = ScanRuntimeLoopSource(
                "SelectNextDevEnemy",
                "selectedDevEnemyIndex = (selectedDevEnemyIndex + 1) % count",
                "ResolveDevEnemyScenario");
            bool runningStateVisible = ScanRuntimeLoopSource(
                "public bool IsRunning",
                "StartLoop()",
                "TickLoop(Time.deltaTime)");
            bool runtimeAllowsPlayerHpZero =
                !RuntimeLoopSourceContains("Mathf.Max(1, playerHp")
                && !RuntimeLoopSourceContains("playerHpAfter <= 0 ? safePreview.finalPlayerHp");

            BattleSandboxPlayableLoopSnapshot snapshot = new()
            {
                devOnly = true,
                isEnabled = false,
                reusesBattleSandboxRuntimeLoop = string.Equals(
                    BattleSandboxPlayableLoop.RuntimeLoopPackageName,
                    BattleSandboxRuntimeLoopPreview.PackageName,
                    StringComparison.Ordinal),
                rewritesBattleLoop = false,
                runtimeRunningStateVisible = runningStateVisible,
                enemyAttackTimerAdvances = (runtimePreview?.EnemyAttackTimerAdvanceRowCount ?? 0) > 0,
                enemyAttackDamageFromDevOnlyProfile = (runtimePreview?.DevOnlyProfileAttackDamageRowCount ?? 0) > 0,
                playerShieldBeforeHpDamage = (runtimePreview?.ShieldFirstPlayerDamageRowCount ?? 0) > 0,
                runtimeAllowsPlayerHpZero = runtimeAllowsPlayerHpZero,
                restartRetainsCurrentBuild = restartRetainsBuild,
                restartResetsHpShieldManaCooldownLogCast = restartResetsRuntime,
                switchDevTargets310410 = supportsTargetSwitch,
                runtimePreviewRowCount = runtimePreview?.rows?.Count ?? 0,
                runtimeManaRowCount = runtimePreview?.ManaRowCount ?? 0,
                runtimeCooldownRowCount = runtimePreview?.CooldownRowCount ?? 0,
                runtimeCombatLogRowCount = runtimePreview?.CombatLogRowCount ?? 0,
                runtimeBossCastRowCount = runtimePreview?.BossCastRowCount ?? 0,
                runtimeSandboxResultRowCount = runtimePreview?.SandboxResultRowCount ?? 0,
                featureFlagDefaultTrueCount = runtimePreview?.FeatureFlagDefaultTrueCount ?? 1,
                modifiesFormalBuildSettings = sceneSnapshot.BuildSettingsContainsPreview,
                touchesFormalUiLayout = runtimePreview?.touchesFormalSceneUiLayout ?? true,
                uiLayoutWriteCount = runtimePreview?.UiLayoutWriteCount ?? 1
            };

            int formalLeakCount = runtimePreview?.FormalLeakCount ?? 1;
            int settlementLeakCount = runtimePreview?.SettlementLeakCount ?? 1;
            int playerLeakCount = runtimePreview?.PlayerSideAnswerLeakCount ?? 1;

            foreach (BattleSandboxRuntimeLoopScenario scenario in scenarios)
            {
                if (scenario == null)
                {
                    continue;
                }

                if (string.Equals(scenario.devChapterLabel, "3-10", StringComparison.Ordinal))
                {
                    snapshot.chapter310TargetCount++;
                }

                if (string.Equals(scenario.devChapterLabel, "4-10", StringComparison.Ordinal))
                {
                    snapshot.chapter410TargetCount++;
                }

                BattleSandboxRuntimeLoopPreview scenarioPreview =
                    BattleSandboxRuntimeLoopPreviewBuilder.Build(
                        defaultLayout,
                        string.IsNullOrWhiteSpace(scenario.previewBuildId)
                            ? "battle_sandbox_playable_loop_scenario"
                            : scenario.previewBuildId,
                        scenario);
                formalLeakCount += scenarioPreview.FormalLeakCount;
                settlementLeakCount += scenarioPreview.SettlementLeakCount;
                playerLeakCount += scenarioPreview.PlayerSideAnswerLeakCount;

                BattleSandboxRuntimeLoopRow resultRow = scenarioPreview.rows?
                    .LastOrDefault(row => row != null && (row.hasSandboxVictoryResult || row.hasSandboxDefeatResult));
                if (resultRow == null)
                {
                    continue;
                }

                BattleSandboxPlayableLoopRow row = BuildPlayableRow(
                    scenario,
                    resultRow,
                    restartRetainsBuild,
                    restartResetsRuntime,
                    supportsTargetSwitch);
                snapshot.rows.Add(row);
            }

            snapshot.victoryRowCount = snapshot.rows.Count(row => row != null && row.sandboxVictory);
            snapshot.defeatRowCount = snapshot.rows.Count(row => row != null && row.sandboxDefeat);
            snapshot.restartRowCount = snapshot.rows.Count(row => row != null && row.restartRetainsCurrentBuild && row.restartResetsRuntimeState);
            snapshot.switchTargetRowCount = snapshot.rows.Count(row => row != null && row.supportsTargetSwitch);
            snapshot.buildFeedbackRowCount = snapshot.rows.Count(row => row != null && row.buildBriefVisible);
            snapshot.victoryConditionEnemyHpZero = snapshot.rows.Any(row => row != null && row.sandboxVictory && row.enemyHpAfter <= 0);
            snapshot.defeatConditionPlayerHpZero = snapshot.rows.Any(row => row != null && row.sandboxDefeat && row.playerHpAfter <= 0);
            snapshot.victoryPromptVisible = snapshot.rows.Any(row => row != null && row.sandboxVictory && row.promptVisible);
            snapshot.defeatPromptVisible = snapshot.rows.Any(row => row != null && row.sandboxDefeat && row.promptVisible);
            snapshot.buildBriefFeedbackVisible = snapshot.buildFeedbackRowCount > 0;
            snapshot.formalLeakCount = formalLeakCount;
            snapshot.settlementLeakCount = settlementLeakCount;
            snapshot.playerSideAnswerLeakCount =
                playerLeakCount
                + sceneSnapshot.ForbiddenAnswerTextViolations.Count
                + snapshot.rows.Count(row => row == null || row.playerSideAnswerLeak);
            return snapshot;
        }

        public static BuildSandboxValidationReport Validate(
            BattleSandboxPlayableLoopSnapshot snapshot = null)
        {
            BattleSandboxPlayableLoopSnapshot safeSnapshot =
                snapshot ?? BuildDefaultSnapshot();
            BuildSandboxValidationReport report = new("BattleSandbox Playable Loop 01");

            RequireTrue(report, "PLAYABLE_LOOP_DEVONLY_TRUE", safeSnapshot.devOnly, "Playable loop remains devOnly.");
            RequireFalse(report, "PLAYABLE_LOOP_ENABLED_FALSE", safeSnapshot.isEnabled, "Playable loop remains disabled by default.");
            RequireTrue(report, "PLAYABLE_LOOP_REUSES_RUNTIME_LOOP", safeSnapshot.reusesBattleSandboxRuntimeLoop, "Playable loop reuses BattleSandboxRuntimeLoop01.");
            RequireFalse(report, "PLAYABLE_LOOP_REWRITE_FALSE", safeSnapshot.rewritesBattleLoop, "Playable loop does not rewrite the battle loop.");
            RequireTrue(report, "PLAYABLE_LOOP_RUNNING_STATE_VISIBLE", safeSnapshot.runtimeRunningStateVisible, "Runtime loop exposes a running state after battle mode starts.");
            RequireTrue(report, "PLAYABLE_LOOP_ATTACK_TIMER_ADVANCES", safeSnapshot.enemyAttackTimerAdvances, "Enemy attack timer advances in runtime rows.");
            RequireTrue(report, "PLAYABLE_LOOP_ATTACK_DAMAGE_PROFILE", safeSnapshot.enemyAttackDamageFromDevOnlyProfile, "Enemy attack damage comes from devOnly profile rows.");
            RequireTrue(report, "PLAYABLE_LOOP_SHIELD_BEFORE_HP", safeSnapshot.playerShieldBeforeHpDamage, "Enemy attack consumes shield before HP.");
            RequireTrue(report, "PLAYABLE_LOOP_HP_ZERO_ALLOWED", safeSnapshot.runtimeAllowsPlayerHpZero, "Playable loop allows player HP to reach zero.");
            RequireTrue(report, "PLAYABLE_LOOP_VICTORY_HP_ZERO", safeSnapshot.victoryConditionEnemyHpZero, "Victory condition is enemy HP <= 0.");
            RequireTrue(report, "PLAYABLE_LOOP_DEFEAT_HP_ZERO", safeSnapshot.defeatConditionPlayerHpZero, "Defeat condition is player HP <= 0.");
            RequireTrue(report, "PLAYABLE_LOOP_RESULT_PROMPTS", safeSnapshot.victoryPromptVisible && safeSnapshot.defeatPromptVisible, "Victory and defeat sandbox prompts are visible.");
            RequireTrue(report, "PLAYABLE_LOOP_BUILD_BRIEF", safeSnapshot.buildBriefFeedbackVisible, "Result prompt includes brief Build feedback.");
            RequireTrue(report, "PLAYABLE_LOOP_RESTART_RETAINS_BUILD", safeSnapshot.restartRetainsCurrentBuild, "Restart retains the current board Build snapshot.");
            RequireTrue(report, "PLAYABLE_LOOP_RESTART_RESETS_STATE", safeSnapshot.restartResetsHpShieldManaCooldownLogCast && safeSnapshot.HasRequiredRuntimeResetCoverage, "Restart resets HP, shield, mana, cooldown, log, and cast surfaces.");
            RequireTrue(report, "PLAYABLE_LOOP_SWITCH_310_410", safeSnapshot.switchDevTargets310410 && safeSnapshot.chapter310TargetCount > 0 && safeSnapshot.chapter410TargetCount > 0, "Target switch covers devOnly 3-10 and 4-10 targets.");
            RequireEquals(report, "PLAYABLE_LOOP_FEATURE_FLAGS_ZERO", "feature flag default true count", safeSnapshot.featureFlagDefaultTrueCount, 0);
            RequireEquals(report, "PLAYABLE_LOOP_FORMAL_LEAK_ZERO", "formal flow leak count", safeSnapshot.formalLeakCount, 0);
            RequireEquals(report, "PLAYABLE_LOOP_SETTLEMENT_LEAK_ZERO", "formal settlement leak count", safeSnapshot.settlementLeakCount, 0);
            RequireEquals(report, "PLAYABLE_LOOP_PLAYER_LEAK_ZERO", "player-side answer leak count", safeSnapshot.playerSideAnswerLeakCount, 0);
            RequireEquals(report, "PLAYABLE_LOOP_UI_LAYOUT_WRITE_ZERO", "UI layout write count", safeSnapshot.uiLayoutWriteCount, 0);
            RequireFalse(report, "PLAYABLE_LOOP_BUILDS_SETTINGS_FALSE", safeSnapshot.modifiesFormalBuildSettings, "Preview scene is not inserted into formal Build Settings.");
            RequireFalse(report, "PLAYABLE_LOOP_V02_V03_TOUCH_FALSE", safeSnapshot.touchesV02OrV03, "Playable loop does not touch V0.2/V0.3 surfaces.");
            RequireFalse(report, "PLAYABLE_LOOP_FORMAL_UI_LAYOUT_FALSE", safeSnapshot.touchesFormalUiLayout, "Playable loop does not author formal UI layout.");

            foreach (BattleSandboxPlayableLoopRow row in safeSnapshot.rows ?? new List<BattleSandboxPlayableLoopRow>())
            {
                if (row == null)
                {
                    report.AddError("PLAYABLE_LOOP_ROW_NULL", "Playable loop row is null.", PackageName);
                    continue;
                }

                if (!row.Passed)
                {
                    report.AddError(
                        "PLAYABLE_LOOP_ROW_FAIL",
                        $"Playable loop row failed. id={row.rowId}, kind={row.rowKind}.",
                        nameof(BattleSandboxPlayableLoopRow));
                }
            }

            return report;
        }

        private static BattleSandboxPlayableLoopRow BuildPlayableRow(
            BattleSandboxRuntimeLoopScenario scenario,
            BattleSandboxRuntimeLoopRow resultRow,
            bool restartRetainsBuild,
            bool restartResetsRuntime,
            bool supportsTargetSwitch)
        {
            string resultText = string.Join(
                " ",
                resultRow.resultTitleChinese,
                resultRow.resultBodyChinese,
                resultRow.combatLogLineChinese,
                resultRow.floatingTextChinese,
                resultRow.restartHintChinese);
            return new BattleSandboxPlayableLoopRow
            {
                rowId = "playableLoop." + (resultRow.rowId ?? string.Empty),
                rowKind = resultRow.rowKind,
                scenarioStageId = scenario?.stageId ?? string.Empty,
                devChapterLabel = scenario?.devChapterLabel ?? string.Empty,
                targetDisplayNameChinese = scenario?.enemyDisplayNameChinese ?? string.Empty,
                enemyHpAfter = resultRow.enemyHpAfter,
                playerHpAfter = resultRow.playerHpAfter,
                sandboxVictory = resultRow.hasSandboxVictoryResult,
                sandboxDefeat = resultRow.hasSandboxDefeatResult,
                promptVisible = !string.IsNullOrWhiteSpace(resultRow.resultTitleChinese)
                    && !string.IsNullOrWhiteSpace(resultRow.resultBodyChinese),
                buildBriefVisible = resultText.IndexOf("\u672c\u5c40 Build \u7b80\u8bc4", StringComparison.Ordinal) >= 0,
                restartRetainsCurrentBuild = restartRetainsBuild,
                restartResetsRuntimeState = restartResetsRuntime,
                supportsTargetSwitch = supportsTargetSwitch,
                playerSideAnswerLeak = resultRow.playerSideAnswerLeak || ContainsForbiddenPlayerToken(resultText),
                formalFlowLeak = resultRow.formalFlowLeak || resultRow.hasVictorySettlement || resultRow.hasDefeatSettlement,
                uiLayoutWrite = resultRow.uiLayoutWrite,
                resultTitleChinese = resultRow.resultTitleChinese,
                resultBodyChinese = resultRow.resultBodyChinese,
                restartHintChinese = resultRow.restartHintChinese
            };
        }

        private static bool ContainsForbiddenPlayerToken(string value)
        {
            string safeValue = value ?? string.Empty;
            return ForbiddenPlayerAnswerTokens.Any(token =>
                safeValue.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static bool ScanRuntimeLoopSource(params string[] requiredTokens)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string path = Path.Combine(
                projectRoot,
                RuntimeLoopSourcePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                return false;
            }

            string text = File.ReadAllText(path);
            return requiredTokens.All(token => text.IndexOf(token, StringComparison.Ordinal) >= 0);
        }

        private static bool RuntimeLoopSourceContains(string token)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string path = Path.Combine(
                projectRoot,
                RuntimeLoopSourcePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(path))
            {
                return false;
            }

            string text = File.ReadAllText(path);
            return text.IndexOf(token ?? string.Empty, StringComparison.Ordinal) >= 0;
        }

        private static void RequireTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string message)
        {
            if (value)
            {
                report.AddInfo(code, message, PackageName);
                return;
            }

            report.AddError(code, message, PackageName);
        }

        private static void RequireFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value,
            string message)
        {
            RequireTrue(report, code, !value, message);
        }

        private static void RequireEquals(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual == expected)
            {
                report.AddInfo(code, $"{label} pass. actual={actual}.", PackageName);
                return;
            }

            report.AddError(code, $"{label} mismatch. actual={actual}, expected={expected}.", PackageName);
        }
    }
}
#endif
