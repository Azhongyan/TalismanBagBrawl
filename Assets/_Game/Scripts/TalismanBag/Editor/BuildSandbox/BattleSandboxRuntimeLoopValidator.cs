#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxRuntimeLoopValidator
    {
        public const string PackageName = BattleSandboxRuntimeLoopPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxRuntimeLoop01/[QA Only] Run Runtime Loop";

        private static readonly string[] ForbiddenPlayerAnswerTokens =
        {
            "bossSixKeyFullAnswer",
            "DropBias",
            "dropBias",
            "requiredSynergy",
            "requiredAffix",
            "requiredStats",
            "formalRunFlow",
            "SaveData",
            "Reward",
            "Chapter"
        };

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            BattleSandboxRuntimeLoopPreview preview = BuildDefaultPreview();
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                BattleSandboxCombatKernelAdapterValidator.Validate(),
                BattleSandboxBuildCombatPreviewValidator.Validate(),
                BattleSandboxManaLoopRuntimeValidator.Validate(),
                Validate(preview)
            };
        }

        public static BattleSandboxRuntimeLoopPreview BuildDefaultPreview()
        {
            return BattleSandboxRuntimeLoopPreviewBuilder.BuildDefaultPreview();
        }

        public static BuildSandboxValidationReport Validate(
            BattleSandboxRuntimeLoopPreview preview = null)
        {
            BuildSandboxValidationReport report = new("BattleSandbox Runtime Loop 01");
            BattleSandboxRuntimeLoopPreview safePreview = preview ?? BuildDefaultPreview();
            ValidateIsolation(report, safePreview);
            ValidateRows(report, safePreview);
            ValidateSources(report, safePreview);
            ValidateDevEnemyScenarioCoverage(report);
            ValidateEmptyBoardDamageGuard(report);
            return report;
        }

        public static IReadOnlyList<BattleSandboxRuntimeLoopRow> Rows(
            BattleSandboxRuntimeLoopPreview preview)
        {
            if (preview?.rows == null)
            {
                return Array.Empty<BattleSandboxRuntimeLoopRow>();
            }

            return preview.rows;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxRuntimeLoopPreview preview)
        {
            if (preview == null)
            {
                report.AddError("RUNTIME_LOOP_PREVIEW_NULL", "Runtime loop preview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "RUNTIME_LOOP_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(BattleSandboxRuntimeLoopPreview));
            }

            RequireTrue(report, "RUNTIME_LOOP_DEVONLY_TRUE", preview.devOnly);
            RequireFalse(report, "RUNTIME_LOOP_ENABLED_FALSE", preview.isEnabled);
            RequireTrue(report, "RUNTIME_LOOP_READS_KERNEL_ADAPTER", preview.readsCombatKernelAdapter);
            RequireTrue(report, "RUNTIME_LOOP_READS_ITEM_STAT", preview.readsBuildSandboxItemStat);
            RequireTrue(report, "RUNTIME_LOOP_READS_CURRENT_BOARD", preview.readsCurrentBoardSnapshot);
            RequireTrue(report, "RUNTIME_LOOP_READS_BUILD_PREVIEW", preview.readsBuildCombatPreview);
            RequireTrue(report, "RUNTIME_LOOP_WRITES_HUD_TEXT", preview.updatesRuntimeHudText);
            RequireTrue(report, "RUNTIME_LOOP_WRITES_FLOATING_TEXT", preview.updatesRuntimeFloatingText);
            RequireTrue(report, "RUNTIME_LOOP_RUNNING_STATE", preview.runtimeLoopStartsWhenBattleModeActive);
            RequireFalse(report, "RUNTIME_LOOP_FORMAL_COMBAT_FALSE", preview.runsFormalCombat);
            RequireFalse(report, "RUNTIME_LOOP_FORMAL_DAMAGE_FALSE", preview.callsFormalDamageSettlement);
            RequireFalse(report, "RUNTIME_LOOP_FLOW_WRITE_FALSE", preview.writesFormalFlow);
            RequireFalse(report, "RUNTIME_LOOP_SAVE_WRITE_FALSE", preview.writesFormalSaveData);
            RequireFalse(report, "RUNTIME_LOOP_REWARD_FALSE", preview.grantsFormalReward);
            RequireFalse(report, "RUNTIME_LOOP_CHAPTER_FALSE", preview.advancesChapter);
            RequireFalse(report, "RUNTIME_LOOP_FEATURE_FLAG_FALSE", preview.opensFeatureFlag);
            RequireFalse(report, "RUNTIME_LOOP_UI_LAYOUT_TOUCH_FALSE", preview.touchesFormalSceneUiLayout);
            RequireFalse(report, "RUNTIME_LOOP_VICTORY_FALSE", preview.hasVictorySettlement);
            RequireFalse(report, "RUNTIME_LOOP_DEFEAT_FALSE", preview.hasDefeatSettlement);

            RequireEquals(report, "RUNTIME_LOOP_FEATURE_FLAGS_ZERO", "feature flag default true count", preview.FeatureFlagDefaultTrueCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_FORMAL_LEAK_ZERO", "formal leak count", preview.FormalLeakCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_SETTLEMENT_ZERO", "victory/defeat settlement count", preview.SettlementLeakCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_PLAYER_LEAK_ZERO", "player-side answer leak count", preview.PlayerSideAnswerLeakCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_UI_LAYOUT_WRITE_ZERO", "UI layout write count", preview.UiLayoutWriteCount, 0);
            RequireTrue(report, "RUNTIME_LOOP_DEVONLY_ISOLATION_PASS", preview.DevOnlyIsolationPass);
        }

        private static void ValidateRows(
            BuildSandboxValidationReport report,
            BattleSandboxRuntimeLoopPreview preview)
        {
            IReadOnlyList<BattleSandboxRuntimeLoopRow> rows = Rows(preview);
            RequireMinimum(report, "RUNTIME_LOOP_ROW_COUNT", "runtime loop row", rows.Count, 12);
            RequireMinimum(report, "RUNTIME_LOOP_MANA_ROWS", "mana row", preview?.ManaRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_COOLDOWN_ROWS", "cooldown row", preview?.CooldownRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_TRIGGER_ROWS", "item trigger row", preview?.ItemTriggerRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_ENEMY_HP_ROWS", "enemy HP row", preview?.EnemyHpRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_ENEMY_SHIELD_ROWS", "enemy shield update row", preview?.EnemyShieldRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_PLAYER_HP_ROWS", "player HP row", preview?.PlayerHpRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_PLAYER_SHIELD_ROWS", "player shield row", preview?.PlayerShieldRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_BOSS_CAST_ROWS", "Boss cast-bar row", preview?.BossCastRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_ATTACK_ROWS", "enemy attack row", preview?.EnemyAttackRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_ATTACK_TIMER_ADVANCES", "enemy attack timer advance row", preview?.EnemyAttackTimerAdvanceRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_ATTACK_DAMAGE_PROFILE_ROWS", "devOnly profile attack damage row", preview?.DevOnlyProfileAttackDamageRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_SHIELD_FIRST_ROWS", "shield-first player damage row", preview?.ShieldFirstPlayerDamageRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_LOG_ROWS", "combat log row", preview?.CombatLogRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_FLOATING_ROWS", "floating text row", preview?.FloatingTextRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_SANDBOX_RESULT_ROWS", "sandbox result row", preview?.SandboxResultRowCount ?? 0, 1);
            RequireEquals(report, "RUNTIME_LOOP_NO_SETTLEMENT_ZERO", "legacy no-settlement row", preview?.NoSettlementRowCount ?? 0, 0);

            foreach (BattleSandboxRuntimeLoopRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("RUNTIME_LOOP_ROW_NULL", "Runtime loop row is null.", PackageName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.rowId)
                    || string.IsNullOrWhiteSpace(row.rowKind)
                    || string.IsNullOrWhiteSpace(row.sourceDataPath)
                    || string.IsNullOrWhiteSpace(row.developerDataPanelFieldKey))
                {
                    report.AddError(
                        "RUNTIME_LOOP_ROW_IDENTITY_MISSING",
                        $"Runtime loop row needs id, kind, source path, and developer field key. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (!string.Equals(row.developerDataPanelFieldKey, BattleSandboxRuntimeLoopPreview.DeveloperDataPanelFieldKey, StringComparison.Ordinal))
                {
                    report.AddError(
                        "RUNTIME_LOOP_DEVELOPER_KEY_MISMATCH",
                        $"Runtime loop row must use {BattleSandboxRuntimeLoopPreview.DeveloperDataPanelFieldKey}. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (row.formalFlowLeak || row.hasVictorySettlement || row.hasDefeatSettlement)
                {
                    report.AddError(
                        "RUNTIME_LOOP_ROW_FORMAL_OR_SETTLEMENT_LEAK",
                        $"Runtime loop row must not connect formal flow or victory/defeat settlement. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if ((row.hasSandboxVictoryResult || row.hasSandboxDefeatResult) && !row.locksRuntimeLoop)
                {
                    report.AddError(
                        "RUNTIME_LOOP_RESULT_MUST_LOCK",
                        $"Sandbox result row must lock the runtime loop. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (row.hasSandboxVictoryResult && row.enemyHpAfter != 0)
                {
                    report.AddError(
                        "RUNTIME_LOOP_VICTORY_ENEMY_ZERO",
                        $"Sandbox victory row should drive enemy HP to zero. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (row.hasSandboxDefeatResult && row.playerHpAfter != 0)
                {
                    report.AddError(
                        "RUNTIME_LOOP_DEFEAT_PLAYER_ZERO",
                        $"Sandbox defeat row should drive player HP to zero. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (row.uiLayoutWrite)
                {
                    report.AddError(
                        "RUNTIME_LOOP_ROW_UI_LAYOUT_WRITE",
                        $"Runtime loop row must not write RectTransform/layout. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (ContainsForbiddenPlayerToken(row))
                {
                    report.AddError(
                        "RUNTIME_LOOP_PLAYER_ANSWER_TOKEN",
                        $"Runtime loop player-facing row contains a forbidden answer/progression token. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }
            }

            ValidateEnemyAttackRows(report, rows);

            report.AddInfo(
                "RUNTIME_LOOP_ROWS_READY",
                $"Rows={rows.Count}, mana={preview?.ManaRowCount ?? 0}, trigger={preview?.ItemTriggerRowCount ?? 0}, bossCast={preview?.BossCastRowCount ?? 0}, attacks={preview?.EnemyAttackRowCount ?? 0}, sandboxResults={preview?.SandboxResultRowCount ?? 0}.",
                nameof(BattleSandboxRuntimeLoopPreview));
        }

        private static void ValidateSources(
            BuildSandboxValidationReport report,
            BattleSandboxRuntimeLoopPreview preview)
        {
            RequireMinimum(report, "RUNTIME_LOOP_SOURCE_KERNEL_ROWS", "CombatKernelAdapter source row", preview?.sourceCombatKernelAdapterRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_SOURCE_BUILD_PREVIEW_ROWS", "BuildCombatPreview source row", preview?.sourceBuildCombatPreviewRowCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_SOURCE_ITEM_STAT_ROWS", "ItemStat source profile", preview?.sourceItemStatProfileCount ?? 0, 1);
            RequireMinimum(report, "RUNTIME_LOOP_SOURCE_ATTACK_DAMAGE", "devOnly enemy/profile attack damage", preview?.selectedDevEnemyAttackDamage ?? 0, 1);
            RequireFalse(report, "RUNTIME_LOOP_RUNTIME_DEFAULT_INPUT_FALSE", preview?.runtimeUsesDefaultLayoutAsCombatInput ?? true);

            if ((preview?.selectedDevEnemyAttackIntervalSeconds ?? 0f) <= 0f)
            {
                report.AddError(
                    "RUNTIME_LOOP_SOURCE_ATTACK_INTERVAL",
                    "Selected dev enemy/profile attack interval must be positive.",
                    nameof(BattleSandboxRuntimeLoopPreview));
            }

            if ((preview?.selectedDevEnemyAttackSourcePath ?? string.Empty).IndexOf("EnemyBossValidationPool", StringComparison.Ordinal) < 0)
            {
                report.AddError(
                    "RUNTIME_LOOP_SOURCE_ATTACK_PROFILE",
                    $"Attack damage must come from EnemyBossValidationPool devOnly profile. source={preview?.selectedDevEnemyAttackSourcePath}.",
                    nameof(BattleSandboxRuntimeLoopPreview));
            }

            foreach (BattleSandboxRuntimeLoopRow row in Rows(preview))
            {
                string source = row?.sourceDataPath ?? string.Empty;
                if (source.IndexOf("RunFlow", StringComparison.OrdinalIgnoreCase) >= 0
                    || source.IndexOf("SaveData", StringComparison.OrdinalIgnoreCase) >= 0
                    || source.IndexOf("Reward", StringComparison.OrdinalIgnoreCase) >= 0
                    || source.IndexOf("Chapter", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    report.AddError(
                        "RUNTIME_LOOP_SOURCE_FORMAL_SURFACE",
                        $"Runtime loop source path must not point at formal flow/data/reward/chapter. id={row?.rowId}, source={source}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }
            }
        }

        private static void ValidateEnemyAttackRows(
            BuildSandboxValidationReport report,
            IReadOnlyList<BattleSandboxRuntimeLoopRow> rows)
        {
            bool defeatBackedByZeroHpAttack = false;
            foreach (BattleSandboxRuntimeLoopRow row in rows ?? Array.Empty<BattleSandboxRuntimeLoopRow>())
            {
                if (row == null)
                {
                    continue;
                }

                if (row.enemyAttackDamage <= 0)
                {
                    continue;
                }

                int expectedBlocked = Math.Min(row.playerShieldBefore, row.enemyAttackDamage);
                int expectedHpDamage = Math.Max(0, row.enemyAttackDamage - expectedBlocked);
                int expectedShieldAfter = Math.Max(0, row.playerShieldBefore - expectedBlocked);
                int expectedHpAfter = Math.Max(0, row.playerHpBefore - expectedHpDamage);

                if (!row.enemyAttackFromDevOnlyProfile
                    || (row.sourceDataPath ?? string.Empty).IndexOf("EnemyBossValidationPool", StringComparison.Ordinal) < 0)
                {
                    report.AddError(
                        "RUNTIME_LOOP_ATTACK_SOURCE_NOT_PROFILE",
                        $"Enemy attack damage must come from a devOnly profile. id={row.rowId}, source={row.sourceDataPath}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (row.playerShieldDamageAbsorbed != expectedBlocked
                    || row.playerHpDamageApplied != expectedHpDamage
                    || row.playerShieldAfter != expectedShieldAfter
                    || row.playerHpAfter != expectedHpAfter
                    || !row.playerShieldDamageResolvedFirst)
                {
                    report.AddError(
                        "RUNTIME_LOOP_ATTACK_SHIELD_FIRST_FAIL",
                        $"Enemy attack must consume shield before HP. id={row.rowId}, damage={row.enemyAttackDamage}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (row.playerHpAfter <= 0)
                {
                    defeatBackedByZeroHpAttack = true;
                }
            }

            bool hasDefeatResult = (rows ?? Array.Empty<BattleSandboxRuntimeLoopRow>())
                .Any(row => row != null && row.hasSandboxDefeatResult);
            if (hasDefeatResult && !defeatBackedByZeroHpAttack)
            {
                report.AddError(
                    "RUNTIME_LOOP_DEFEAT_REQUIRES_ZERO_HP_ATTACK",
                    "Sandbox defeat must be backed by an enemy attack row that drives playerHp <= 0.",
                    nameof(BattleSandboxRuntimeLoopRow));
            }
        }

        private static void ValidateDevEnemyScenarioCoverage(
            BuildSandboxValidationReport report)
        {
            IReadOnlyList<BattleSandboxRuntimeLoopScenario> scenarios =
                BattleSandboxRuntimeLoopPreviewBuilder.BuildDevEnemyScenarios();
            RequireMinimum(report, "RUNTIME_LOOP_DEV_ENEMY_SCENARIOS", "dev enemy scenario", scenarios.Count, 2);

            int chapter310Count = 0;
            int chapter410Count = 0;
            int victoryCount = 0;
            int defeatCount = 0;
            BuildSandboxLayoutSnapshot snapshot =
                BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreviewLayoutSnapshot();

            foreach (BattleSandboxRuntimeLoopScenario scenario in scenarios)
            {
                if (scenario == null)
                {
                    continue;
                }

                if (string.Equals(scenario.devChapterLabel, "3-10", StringComparison.Ordinal))
                {
                    chapter310Count++;
                }

                if (string.Equals(scenario.devChapterLabel, "4-10", StringComparison.Ordinal))
                {
                    chapter410Count++;
                }

                BattleSandboxRuntimeLoopPreview preview =
                    BattleSandboxRuntimeLoopPreviewBuilder.Build(
                        snapshot,
                        string.IsNullOrWhiteSpace(scenario.previewBuildId)
                            ? "runtime_loop_scenario_validation"
                            : scenario.previewBuildId,
                        scenario);
                victoryCount += preview.SandboxVictoryResultRowCount;
                defeatCount += preview.SandboxDefeatResultRowCount;

                if (scenario.attackDamage <= 0)
                {
                    report.AddError(
                        "RUNTIME_LOOP_SCENARIO_ATTACK_DAMAGE_ZERO",
                        $"Scenario attackDamage must be > 0. id={scenario.stageId}.",
                        nameof(BattleSandboxRuntimeLoopScenario));
                }

                if (scenario.attackIntervalSeconds <= 0f)
                {
                    report.AddError(
                        "RUNTIME_LOOP_SCENARIO_ATTACK_INTERVAL_INVALID",
                        $"Scenario attackIntervalSeconds must be positive. id={scenario.stageId}.",
                        nameof(BattleSandboxRuntimeLoopScenario));
                }

                if (!scenario.attackFromDevOnlyProfile
                    || (scenario.attackSourcePath ?? string.Empty).IndexOf("EnemyBossValidationPool", StringComparison.Ordinal) < 0)
                {
                    report.AddError(
                        "RUNTIME_LOOP_SCENARIO_ATTACK_SOURCE",
                        $"Scenario attack must resolve from devOnly EnemyBossValidationPool profile. id={scenario.stageId}.",
                        nameof(BattleSandboxRuntimeLoopScenario));
                }

                if (preview.FormalLeakCount > 0 || preview.SettlementLeakCount > 0)
                {
                    report.AddError(
                        "RUNTIME_LOOP_SCENARIO_FORMAL_LEAK",
                        $"Scenario generated a formal leak. id={scenario.stageId}.",
                        nameof(BattleSandboxRuntimeLoopScenario));
                }
            }

            RequireMinimum(report, "RUNTIME_LOOP_DEV_ENEMY_310", "3-10 dev enemy scenario", chapter310Count, 1);
            RequireMinimum(report, "RUNTIME_LOOP_DEV_ENEMY_410", "4-10 dev enemy scenario", chapter410Count, 1);
            RequireMinimum(report, "RUNTIME_LOOP_SANDBOX_VICTORY_COVERAGE", "sandbox victory scenario", victoryCount, 1);
            RequireMinimum(report, "RUNTIME_LOOP_SANDBOX_DEFEAT_COVERAGE", "sandbox defeat scenario", defeatCount, 1);
        }

        private static void ValidateEmptyBoardDamageGuard(
            BuildSandboxValidationReport report)
        {
            BattleSandboxRuntimeLoopPreview emptyPreview =
                BattleSandboxRuntimeLoopPreviewBuilder.Build(
                    new BuildSandboxLayoutSnapshot(),
                    "empty_board_runtime_damage_guard",
                    BattleSandboxRuntimeLoopPreviewBuilder.ResolveDevEnemyScenario(0));

            RequireEquals(report, "RUNTIME_LOOP_EMPTY_PLACED_ITEMS_ZERO", "empty board placed item count", emptyPreview.currentBoardPlacedItemCount, 0);
            RequireFalse(report, "RUNTIME_LOOP_EMPTY_NO_DEFAULT_LAYOUT", emptyPreview.runtimeUsesDefaultLayoutAsCombatInput);
            RequireFalse(report, "RUNTIME_LOOP_EMPTY_NO_FALLBACK", emptyPreview.usesDefaultLayoutFallbackWhenBoardEmpty);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_ITEM_STAT_ZERO", "empty board item stat source count", emptyPreview.sourceItemStatProfileCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_TRIGGER_ROWS_ZERO", "empty board item trigger row count", emptyPreview.ItemTriggerRowCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_COOLDOWN_ROWS_ZERO", "empty board item cooldown row count", emptyPreview.CooldownRowCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_ENEMY_HP_ROWS_ZERO", "empty board enemy HP damage row count", emptyPreview.EnemyHpRowCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_PLAYER_ITEM_DAMAGE_ZERO", "empty board player item enemy HP damage", emptyPreview.playerItemEnemyHpDamageTotal, 0);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_ENEMY_HP_UNCHANGED", "empty board final enemy HP", emptyPreview.finalEnemyHp, emptyPreview.initialEnemyHp);
            RequireMinimum(report, "RUNTIME_LOOP_EMPTY_PLAYER_HP_ROWS", "empty board player HP row", emptyPreview.PlayerHpRowCount, 1);
            RequireMinimum(report, "RUNTIME_LOOP_EMPTY_DEFEAT_RESULT", "empty board sandbox defeat row", emptyPreview.SandboxDefeatResultRowCount, 1);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_VICTORY_ZERO", "empty board sandbox victory row", emptyPreview.SandboxVictoryResultRowCount, 0);
            RequireEquals(report, "RUNTIME_LOOP_EMPTY_FINAL_PLAYER_HP_ZERO", "empty board final player HP", emptyPreview.finalPlayerHp, 0);

            foreach (BattleSandboxRuntimeLoopRow row in Rows(emptyPreview))
            {
                if (row == null)
                {
                    continue;
                }

                if (row.enemyHpBefore != row.enemyHpAfter)
                {
                    report.AddError(
                        "RUNTIME_LOOP_EMPTY_ENEMY_HP_CHANGED",
                        $"Empty board must not change enemy HP. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }

                if (!string.IsNullOrWhiteSpace(row.itemId)
                    || !string.IsNullOrWhiteSpace(row.statProfileId))
                {
                    report.AddError(
                        "RUNTIME_LOOP_EMPTY_ITEM_ROW_LEAK",
                        $"Empty board runtime row must not carry item/stat data. id={row.rowId}.",
                        nameof(BattleSandboxRuntimeLoopRow));
                }
            }
        }

        private static bool ContainsForbiddenPlayerToken(BattleSandboxRuntimeLoopRow row)
        {
            string combined = string.Join(
                " ",
                row.stateLineChinese,
                row.castSkillLineChinese,
                row.combatLogLineChinese,
                row.floatingTextChinese);
            foreach (string token in ForbiddenPlayerAnswerTokens)
            {
                if (combined.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return row.playerSideAnswerLeak;
        }

        private static void RequireTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (!value)
            {
                report.AddError(code, "Expected true for this runtime loop flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Required flag remains true.", PackageName);
        }

        private static void RequireFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "Expected false for this runtime loop isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
        }

        private static void RequireEquals(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual != expected)
            {
                report.AddError(code, $"{label} mismatch. actual={actual}, expected={expected}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} pass. actual={actual}.", PackageName);
        }

        private static void RequireMinimum(
            BuildSandboxValidationReport report,
            string code,
            string label,
            int actual,
            int expected)
        {
            if (actual < expected)
            {
                report.AddError(code, $"{label} count too low. actual={actual}, expected>={expected}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} count pass. actual={actual}, expected>={expected}.", PackageName);
        }
    }
}
#endif
