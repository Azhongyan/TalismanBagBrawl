#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxBuildCombatPreviewValidator
    {
        public const string PackageName = BattleSandboxBuildCombatPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxBuildCombatPreview01/[QA Only] Run Build Combat Preview";

        public static List<BuildSandboxValidationReport> BuildValidationReports()
        {
            return new List<BuildSandboxValidationReport>
            {
                BuildSandboxUiLayoutGuard.Validate(),
                Validate()
            };
        }

        public static BattleSandboxBuildCombatPreview BuildDefaultPreview()
        {
            return BattleSandboxBuildCombatPreviewBuilder.BuildDefaultPreview();
        }

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("BattleSandbox Build Combat Preview 01");
            BattleSandboxBuildCombatPreview preview = BuildDefaultPreview();

            ValidateIsolation(report, preview);
            ValidatePreviewData(report, preview);
            ValidateFeedbackRows(report, preview);
            ValidateLeakCounters(report, preview);
            return report;
        }

        public static int CountPlayerTextLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return preview?.PlayerSideAnswerLeakCount ?? 1;
        }

        public static int CountFormalFlowLeaks(BattleSandboxBuildCombatPreview preview)
        {
            return preview?.FormalFlowLeakCount ?? 1;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            if (preview == null)
            {
                report.AddError("BUILD_COMBAT_PREVIEW_NULL", "Build combat preview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "BUILD_COMBAT_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            ValidateTrue(report, "BUILD_COMBAT_DEVONLY_TRUE", preview.devOnly);
            ValidateFalse(report, "BUILD_COMBAT_ENABLED_FALSE", preview.isEnabled);
            ValidateTrue(report, "BUILD_COMBAT_READS_BOARD", preview.readsCurrentBoardSnapshot);
            ValidateTrue(report, "BUILD_COMBAT_READS_DEVONLY_PROBLEM", preview.readsDevOnlyEnemyBossProblem);
            ValidateTrue(report, "BUILD_COMBAT_CALCULATES_SYNERGY", preview.calculatesSynergyPreview);
            ValidateTrue(report, "BUILD_COMBAT_CALCULATES_MODIFIER", preview.calculatesModifierPreview);
            ValidateTrue(report, "BUILD_COMBAT_CALCULATES_READINESS", preview.calculatesReadinessPreview);
            ValidateTrue(report, "BUILD_COMBAT_CALCULATES_SHAPE_RULES", preview.calculatesShapeBuildRulePreview);
            ValidateTrue(report, "BUILD_COMBAT_WRITES_BOSS_STATE", preview.writesBossStateShortLine);
            ValidateTrue(report, "BUILD_COMBAT_WRITES_CAST_BAR", preview.writesCastBar);
            ValidateTrue(report, "BUILD_COMBAT_WRITES_FLOATING", preview.writesMechanicFloatingText);
            ValidateTrue(report, "BUILD_COMBAT_WRITES_LOG", preview.writesCombatFeedbackText);
            ValidateFalse(report, "BUILD_COMBAT_RUNS_FORMAL_COMBAT", preview.runsFormalCombat);
            ValidateFalse(report, "BUILD_COMBAT_CALLS_FORMAL_DAMAGE", preview.callsFormalDamageSettlement);
            ValidateFalse(report, "BUILD_COMBAT_WRITES_FORMAL_FLOW", preview.writesFormalFlow);
            ValidateFalse(report, "BUILD_COMBAT_WRITES_SAVE", preview.writesFormalSaveData);
            ValidateFalse(report, "BUILD_COMBAT_GRANTS_REWARD", preview.grantsFormalReward);
            ValidateFalse(report, "BUILD_COMBAT_ADVANCES_CHAPTER", preview.advancesChapter);
            ValidateFalse(report, "BUILD_COMBAT_OPENS_FEATURE_FLAG", preview.opensFeatureFlag);
            ValidateFalse(report, "BUILD_COMBAT_SHOWS_FULL_ANSWERS", preview.playerUiShowsFullAnswers);

            if (!BuildSandboxFeatureFlags.AreAllDefaultsDisabled())
            {
                report.AddError(
                    "BUILD_COMBAT_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must keep default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if (!preview.DevOnlyIsolationPass)
            {
                report.AddError(
                    "BUILD_COMBAT_DEVONLY_ISOLATION_FAIL",
                    "Preview, context, and feedback preview must stay devOnly=true and isEnabled=false.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            if (preview.shapeBuildRulePreview == null
                || !preview.shapeBuildRulePreview.DevOnlyIsolationPass)
            {
                report.AddError(
                    "BUILD_COMBAT_SHAPE_RULE_ISOLATION_FAIL",
                    "Shape build rule preview must stay devOnly=true, disabled, and disconnected from formal flow.",
                    nameof(BattleSandboxShapeBuildRulePreview));
            }
        }

        private static void ValidatePreviewData(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            ValidateMinimum(report, "BUILD_COMBAT_SCENARIO_COUNT", "Preview scenario", preview?.PreviewScenarioCount ?? 0, 1);
            ValidateMinimum(report, "BUILD_COMBAT_PLACED_ITEM_COUNT", "Placed item snapshot", preview?.PlacedItemSnapshotCount ?? 0, 1);
            ValidateMinimum(report, "BUILD_COMBAT_SYNERGY_COUNT", "Active synergy", preview?.SynergyMatchCount ?? 0, 1);
            ValidateMinimum(report, "BUILD_COMBAT_MODIFIER_COUNT", "Modifier preview", preview?.ModifierBundleCount ?? 0, 1);
            ValidateMinimum(report, "BUILD_COMBAT_EVENT_COUNT", "Effect event preview", preview?.EffectEventCount ?? 0, 1);
            ValidateMinimum(report, "BUILD_COMBAT_SHAPE_RULE_DEFINITION_COUNT", "Shape build rule definition", preview?.ShapeBuildRuleDefinitionCount ?? 0, 4);
            ValidateMinimum(report, "BUILD_COMBAT_SHAPE_RULE_MATCH_COUNT", "Shape build rule match", preview?.ShapeBuildRuleMatchCount ?? 0, 3);
            ValidateMinimum(report, "BUILD_COMBAT_SHAPE_RULE_FEEDBACK_COUNT", "Shape build rule feedback row", preview?.ShapeBuildRuleFeedbackRowCount ?? 0, 3);
            ValidateMinimum(report, "BUILD_COMBAT_BOSS_READINESS_COUNT", "Boss readiness row", preview?.BossReadinessCount ?? 0, 1);

            BuildSandboxPreviewContext context = preview?.context;
            if (context == null
                || context.layoutSnapshot == null
                || context.problemSeedDataset == null
                || context.enemyBossValidationPool == null)
            {
                report.AddError(
                    "BUILD_COMBAT_CONTEXT_SOURCE_MISSING",
                    "Preview context must include board snapshot, problem seed data, and devOnly enemy/Boss pool.",
                    nameof(BuildSandboxPreviewContext));
                return;
            }

            if (!context.devOnly
                || context.isEnabled
                || context.readsFormalSaveData
                || context.writesFormalFlow
                || context.writesFormalData
                || context.touchesFormalScene)
            {
                report.AddError(
                    "BUILD_COMBAT_CONTEXT_SCOPE_LEAK",
                    "Context must remain devOnly, disabled, and disconnected from formal flow/data/scene surfaces.",
                    nameof(BuildSandboxPreviewContext));
            }
        }

        private static void ValidateFeedbackRows(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            IReadOnlyList<BattleSandboxBuildCombatPreviewRow> rows =
                preview?.rows ?? new List<BattleSandboxBuildCombatPreviewRow>();
            ValidateMinimum(report, "BUILD_COMBAT_ROW_COUNT", "Combat preview feedback row", rows.Count, 4);

            if (!rows.Any(row => row.feedbackKind == BattleSandboxEnemyCombatFeedbackKinds.BossState))
            {
                report.AddError("BUILD_COMBAT_BOSS_STATE_MISSING", "Boss state short-line row is missing.", PackageName);
            }

            if (!rows.Any(row => row.feedbackKind == BattleSandboxEnemyCombatFeedbackKinds.BossSkillCast))
            {
                report.AddError("BUILD_COMBAT_CAST_ROW_MISSING", "Cast-bar feedback row is missing.", PackageName);
            }

            if (!rows.Any(row => row.feedbackKind == BattleSandboxEnemyCombatFeedbackKinds.MechanicFeedback))
            {
                report.AddError("BUILD_COMBAT_MECHANIC_ROW_MISSING", "Mechanic floating feedback row is missing.", PackageName);
            }

            int shapeRuleRows = rows.Count(row =>
                row != null
                && !string.IsNullOrWhiteSpace(row.sourceDataPath)
                && row.sourceDataPath.IndexOf("shapeBuildRules", StringComparison.OrdinalIgnoreCase) >= 0);
            ValidateMinimum(report, "BUILD_COMBAT_SHAPE_RULE_ROWS", "Shape build rule player feedback row", shapeRuleRows, 3);

            foreach (BattleSandboxBuildCombatPreviewRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("BUILD_COMBAT_ROW_NULL", "Null combat preview row.", nameof(BattleSandboxBuildCombatPreviewRow));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.feedbackId)
                    || string.IsNullOrWhiteSpace(row.feedbackKind)
                    || string.IsNullOrWhiteSpace(row.sourceDataPath))
                {
                    report.AddError(
                        "BUILD_COMBAT_ROW_IDENTITY_MISSING",
                        $"Feedback row needs id, kind, and source path. id={row.feedbackId}.",
                        nameof(BattleSandboxBuildCombatPreviewRow));
                }

                if (row.playerSideAnswerLeak)
                {
                    report.AddError(
                        "BUILD_COMBAT_PLAYER_ANSWER_LEAK",
                        $"Player-facing feedback leaks an answer token or Latin text. id={row.feedbackId}.",
                        nameof(BattleSandboxBuildCombatPreviewRow));
                }

                if (row.formalFlowLeak)
                {
                    report.AddError(
                        "BUILD_COMBAT_FORMAL_ROW_LEAK",
                        $"Feedback row must not connect formal combat/flow/save/reward/chapter/flags. id={row.feedbackId}.",
                        nameof(BattleSandboxBuildCombatPreviewRow));
                }
            }
        }

        private static void ValidateLeakCounters(
            BuildSandboxValidationReport report,
            BattleSandboxBuildCombatPreview preview)
        {
            if ((preview?.FeatureFlagDefaultTrueCount ?? 1) != 0)
            {
                report.AddError(
                    "BUILD_COMBAT_FEATURE_FLAG_DEFAULT_TRUE",
                    $"Feature flag default true count must be 0. actual={preview?.FeatureFlagDefaultTrueCount ?? 1}.",
                    nameof(BuildSandboxFeatureFlags));
            }

            if (CountPlayerTextLeaks(preview) != 0)
            {
                report.AddError(
                    "BUILD_COMBAT_PLAYER_LEAK_COUNT_NONZERO",
                    $"Player-side answer leak count must be 0. actual={CountPlayerTextLeaks(preview)}.",
                    nameof(BattleSandboxBuildCombatPreview));
            }

            int shapeRulePlayerLeaks = preview?.shapeBuildRulePreview?.PlayerSideAnswerLeakCount ?? 1;
            if (shapeRulePlayerLeaks != 0)
            {
                report.AddError(
                    "BUILD_COMBAT_SHAPE_RULE_PLAYER_LEAK_COUNT_NONZERO",
                    $"Shape build rule player text leak count must be 0. actual={shapeRulePlayerLeaks}.",
                    nameof(BattleSandboxShapeBuildRulePreview));
            }

            if (CountFormalFlowLeaks(preview) != 0)
            {
                report.AddError(
                    "BUILD_COMBAT_FORMAL_LEAK_COUNT_NONZERO",
                    $"Formal flow leak count must be 0. actual={CountFormalFlowLeaks(preview)}.",
                    nameof(BattleSandboxBuildCombatPreview));
            }
        }

        private static void ValidateTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (!value)
            {
                report.AddError(code, "Expected true for this build combat preview flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Required flag remains true.", PackageName);
        }

        private static void ValidateFalse(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (value)
            {
                report.AddError(code, "Expected false for this build combat preview isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
        }

        private static void ValidateMinimum(
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
