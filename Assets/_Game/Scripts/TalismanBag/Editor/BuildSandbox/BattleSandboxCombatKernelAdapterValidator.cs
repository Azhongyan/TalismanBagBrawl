#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.BuildSandbox;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class BattleSandboxCombatKernelAdapterValidator
    {
        public const string PackageName = BattleSandboxCombatKernelAdapterPreview.PackageName;
        public const string QaMenuPath =
            "Tools/Talisman Bag/V0.4/BuildSandbox/BattleSandboxCombatKernelAdapter01/[QA Only] Run Combat Kernel Adapter";

        private static readonly string[] RequiredRuleKeys =
        {
            "spiritPower",
            "cooldown",
            "damage",
            "playerShield",
            "healing",
            "enemyHp",
            "enemySkillCastBar",
            "statusTick",
            "adapterIsolation"
        };

        [MenuItem(QaMenuPath)]
        public static void RunMenu()
        {
            BattleSandboxCombatKernelAdapterPreview preview = BuildDefaultPreview();
            List<BuildSandboxValidationReport> reports = BuildValidationReports(preview);
            string[] paths = BattleSandboxCombatKernelAdapterReportWriter.WriteReports(reports, preview);

            int errors = reports.Sum(report => report.ErrorCount);
            int warnings = reports.Sum(report => report.WarningCount);
            string message =
                $"{PackageName} validation complete. errors={errors}, warnings={warnings}, reports={string.Join(", ", paths)}";
            if (errors > 0)
            {
                Debug.LogError(message);
                return;
            }

            Debug.Log(message);
        }

        public static List<BuildSandboxValidationReport> BuildValidationReports(
            BattleSandboxCombatKernelAdapterPreview preview = null)
        {
            return new List<BuildSandboxValidationReport>
            {
                Validate(preview ?? BuildDefaultPreview())
            };
        }

        public static BattleSandboxCombatKernelAdapterPreview BuildDefaultPreview()
        {
            return BattleSandboxCombatKernelAdapterBuilder.BuildDefaultPreview();
        }

        public static BuildSandboxValidationReport Validate(
            BattleSandboxCombatKernelAdapterPreview preview = null)
        {
            BuildSandboxValidationReport report = new("BattleSandbox Combat Kernel Adapter 01");
            BattleSandboxCombatKernelAdapterPreview safePreview = preview ?? BuildDefaultPreview();

            ValidateIsolation(report, safePreview);
            ValidateRows(report, safePreview);
            ValidateSamples(report, safePreview);
            return report;
        }

        private static void ValidateIsolation(
            BuildSandboxValidationReport report,
            BattleSandboxCombatKernelAdapterPreview preview)
        {
            if (preview == null)
            {
                report.AddError("COMBAT_KERNEL_ADAPTER_NULL", "Combat kernel adapter preview was not created.", PackageName);
                return;
            }

            if (!string.Equals(preview.packageName, PackageName, StringComparison.Ordinal))
            {
                report.AddError(
                    "COMBAT_KERNEL_PACKAGE_MISMATCH",
                    $"Package mismatch. actual={preview.packageName}.",
                    nameof(BattleSandboxCombatKernelAdapterPreview));
            }

            ValidateTrue(report, "COMBAT_KERNEL_DEVONLY_TRUE", preview.devOnly);
            ValidateFalse(report, "COMBAT_KERNEL_ENABLED_FALSE", preview.isEnabled);
            ValidateTrue(report, "COMBAT_KERNEL_ADAPTER_ONLY", preview.adapterOnly);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_RULE_MOUTHFEEL", preview.readsV02V03RuleMouthfeel);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_SPIRIT", preview.reusesSpiritPowerRules);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_COOLDOWN", preview.reusesCooldownRules);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_DAMAGE", preview.reusesDamageRules);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_SHIELD", preview.reusesShieldRules);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_HEALING", preview.reusesHealingRules);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_ENEMY_HP", preview.reusesEnemyHpRules);
            ValidateTrue(report, "COMBAT_KERNEL_REUSES_CAST_BAR", preview.reusesEnemySkillCastBarRules);
            ValidateFalse(report, "COMBAT_KERNEL_RUNFLOW_FALSE", preview.callsFormalRunFlow);
            ValidateFalse(report, "COMBAT_KERNEL_READS_SAVE_FALSE", preview.readsFormalSaveData);
            ValidateFalse(report, "COMBAT_KERNEL_WRITES_SAVE_FALSE", preview.writesFormalSaveData);
            ValidateFalse(report, "COMBAT_KERNEL_REWARD_FALSE", preview.grantsFormalReward);
            ValidateFalse(report, "COMBAT_KERNEL_CHAPTER_FALSE", preview.advancesChapter);
            ValidateFalse(report, "COMBAT_KERNEL_UI_REWRITE_FALSE", preview.rewritesV04Ui);
            ValidateFalse(report, "COMBAT_KERNEL_STABLE_RUNTIME_WRITE_FALSE", preview.modifiesV02V03Runtime);
            ValidateFalse(report, "COMBAT_KERNEL_FORMAL_DAMAGE_FALSE", preview.callsFormalDamageSettlement);
            ValidateFalse(report, "COMBAT_KERNEL_FEATURE_FLAG_FALSE", preview.opensFeatureFlag);

            if (!preview.FeatureFlagsAllDisabled)
            {
                report.AddError(
                    "COMBAT_KERNEL_FEATURE_FLAG_TRUE",
                    "All BuildSandbox feature flags must remain default false.",
                    nameof(BuildSandboxFeatureFlags));
            }

            ValidateEquals(report, "COMBAT_KERNEL_FORMAL_LEAK_ZERO", "formal flow leak count", preview.FormalFlowLeakCount, 0);
            ValidateEquals(report, "COMBAT_KERNEL_UI_LEAK_ZERO", "UI layout leak count", preview.UiLayoutLeakCount, 0);
            ValidateEquals(report, "COMBAT_KERNEL_RULE_SCOPE_LEAK_ZERO", "rule scope leak count", preview.RuleScopeLeakCount, 0);
            ValidateTrue(report, "COMBAT_KERNEL_DEVONLY_ISOLATION_PASS", preview.DevOnlyIsolationPass);
        }

        private static void ValidateRows(
            BuildSandboxValidationReport report,
            BattleSandboxCombatKernelAdapterPreview preview)
        {
            IReadOnlyList<BattleSandboxCombatKernelAdapterRow> rows =
                preview?.rows ?? new List<BattleSandboxCombatKernelAdapterRow>();
            ValidateMinimum(report, "COMBAT_KERNEL_ROW_COUNT", "adapter rule row", rows.Count, RequiredRuleKeys.Length);

            HashSet<string> found = new(rows
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.ruleKey))
                .Select(row => row.ruleKey), StringComparer.Ordinal);
            foreach (string required in RequiredRuleKeys)
            {
                if (!found.Contains(required))
                {
                    report.AddError(
                        "COMBAT_KERNEL_RULE_MISSING",
                        $"Required adapter rule missing. key={required}.",
                        PackageName);
                }
            }

            foreach (BattleSandboxCombatKernelAdapterRow row in rows)
            {
                if (row == null)
                {
                    report.AddError("COMBAT_KERNEL_ROW_NULL", "Null adapter rule row.", PackageName);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.ruleKey)
                    || string.IsNullOrWhiteSpace(row.chineseDisplayName)
                    || string.IsNullOrWhiteSpace(row.sourceReference)
                    || string.IsNullOrWhiteSpace(row.adapterMouthfeel)
                    || string.IsNullOrWhiteSpace(row.sampleInput)
                    || string.IsNullOrWhiteSpace(row.sampleOutput))
                {
                    report.AddError(
                        "COMBAT_KERNEL_ROW_IDENTITY_MISSING",
                        $"Adapter row needs key, display name, source, mouthfeel, sample input, and sample output. key={row.ruleKey}.",
                        nameof(BattleSandboxCombatKernelAdapterRow));
                }

                if (!row.ScopePass)
                {
                    report.AddError(
                        "COMBAT_KERNEL_ROW_SCOPE_LEAK",
                        $"Adapter row must stay devOnly, disabled, and free of flow/UI/stable-runtime leaks. key={row.ruleKey}.",
                        nameof(BattleSandboxCombatKernelAdapterRow));
                }
            }
        }

        private static void ValidateSamples(
            BuildSandboxValidationReport report,
            BattleSandboxCombatKernelAdapterPreview preview)
        {
            BattleSandboxCombatKernelSampleSnapshot sample = preview?.sample;
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_SAMPLE_NULL", "Sample snapshot is missing.", PackageName);
                return;
            }

            ValidatePowerSample(report, sample.power);
            ValidateCooldownSample(report, sample.cooldown);
            ValidateDamageSample(report, sample.damage);
            ValidateShieldSample(report, sample.playerShield);
            ValidateHealingSample(report, sample.healing);
            ValidateEnemyHpSample(report, sample.enemyHp);
            ValidateCastBarSample(report, sample.castBar);
            ValidateStatusTickSample(report, sample.statusTick);
        }

        private static void ValidatePowerSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelPowerSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_POWER_SAMPLE_NULL", "Power sample is missing.", PackageName);
                return;
            }

            ValidateEquals(report, "COMBAT_KERNEL_POWER_CROSS_FULL", "eye cross state", (int)sample.eyeCrossState, (int)BattleSandboxKernelPowerState.FullPowered);
            ValidateEquals(report, "COMBAT_KERNEL_POWER_DIAGONAL_WEAK", "eye diagonal state", (int)sample.eyeDiagonalState, (int)BattleSandboxKernelPowerState.WeakPowered);
            ValidateEquals(report, "COMBAT_KERNEL_POWER_SPIRIT_FULL", "spirit nine-grid state", (int)sample.spiritNineGridState, (int)BattleSandboxKernelPowerState.FullPowered);
            ValidateApprox(report, "COMBAT_KERNEL_POWER_WEAK_MULTIPLIER", "weak power cooldown multiplier", sample.weakPoweredCooldownMultiplier, 1.35f);
        }

        private static void ValidateCooldownSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelCooldownSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_COOLDOWN_SAMPLE_NULL", "Cooldown sample is missing.", PackageName);
                return;
            }

            ValidateApprox(report, "COMBAT_KERNEL_COOLDOWN_MIN", "minimum cooldown", sample.minClampedCooldown, 0.1f);
            ValidateApprox(report, "COMBAT_KERNEL_COOLDOWN_FIRE", "fire adjacent spirit cooldown", sample.fullPoweredFireCooldown, 1.6f);
            ValidateApprox(report, "COMBAT_KERNEL_COOLDOWN_WEAK", "weak fire cooldown", sample.weakPoweredFireCooldown, 2.16f);
            ValidateApprox(report, "COMBAT_KERNEL_COOLDOWN_TRAINED_WEAK", "trained weak fire cooldown", sample.trainedWeakPoweredFireCooldown, 1.944f);
        }

        private static void ValidateDamageSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelDamageSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_DAMAGE_SAMPLE_NULL", "Damage sample is missing.", PackageName);
                return;
            }

            ValidateEquals(report, "COMBAT_KERNEL_DAMAGE_BLOCKED", "shield blocked damage", sample.blockedByShield, 8);
            ValidateEquals(report, "COMBAT_KERNEL_DAMAGE_HP", "HP damage", sample.hpDamage, 12);
            ValidateEquals(report, "COMBAT_KERNEL_DAMAGE_SHIELD_AFTER", "enemy shield after damage", sample.enemyShieldAfter, 0);
            ValidateEquals(report, "COMBAT_KERNEL_DAMAGE_HP_AFTER", "enemy HP after damage", sample.enemyHpAfter, 108);
        }

        private static void ValidateShieldSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelShieldSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_SHIELD_SAMPLE_NULL", "Shield sample is missing.", PackageName);
                return;
            }

            ValidateEquals(report, "COMBAT_KERNEL_PLAYER_SHIELD_CAP", "player shield after cap", sample.playerShieldAfter, 50);
            ValidateEquals(report, "COMBAT_KERNEL_PLAYER_SHIELD_WASTE", "wasted player shield", sample.wastedShield, 13);
            ValidateEquals(report, "COMBAT_KERNEL_ENEMY_SHIELD_ADD", "enemy shield additive result", sample.enemyShieldAfter, 12);
        }

        private static void ValidateHealingSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelHealingSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_HEAL_SAMPLE_NULL", "Healing sample is missing.", PackageName);
                return;
            }

            ValidateEquals(report, "COMBAT_KERNEL_HEAL_CLAMP", "player HP after heal", sample.playerHpAfter, 100);
            ValidateEquals(report, "COMBAT_KERNEL_HEAL_OVERHEAL", "overheal", sample.overheal, 10);
        }

        private static void ValidateEnemyHpSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelEnemyHpSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_ENEMY_SAMPLE_NULL", "Enemy HP sample is missing.", PackageName);
                return;
            }

            ValidateEquals(report, "COMBAT_KERNEL_ENEMY_SPAWN_HP", "enemy spawn HP", sample.currentHpAtSpawn, 120);
            ValidateEquals(report, "COMBAT_KERNEL_ENEMY_SPAWN_SHIELD", "enemy spawn shield", sample.shieldAtSpawn, 0);
            ValidateApprox(report, "COMBAT_KERNEL_ENEMY_ATTACK_INTERVAL", "enemy attack interval", sample.attackInterval, 2f);
            ValidateEquals(report, "COMBAT_KERNEL_ENEMY_ENRAGE_THRESHOLD", "boss enrage HP threshold", sample.bossEnrageHpThreshold, 60);
        }

        private static void ValidateCastBarSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelCastBarSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_CAST_SAMPLE_NULL", "Cast-bar sample is missing.", PackageName);
                return;
            }

            ValidateApprox(report, "COMBAT_KERNEL_CAST_REMAINING", "cast remaining after tick", sample.castTimeRemainingAfterTick, 0.75f);
            ValidateApprox(report, "COMBAT_KERNEL_CAST_BAR_RATIO", "cast bar remaining ratio", sample.castBarRemainingRatioAfterTick, 0.6f);
            ValidateApprox(report, "COMBAT_KERNEL_CAST_COOLDOWN", "cooldown after complete", sample.cooldownAfterComplete, 4f);
            ValidateApprox(report, "COMBAT_KERNEL_CAST_ZERO_COOLDOWN_CLAMP", "zero cooldown clamp", sample.zeroCooldownAfterComplete, 0.1f);
        }

        private static void ValidateStatusTickSample(
            BuildSandboxValidationReport report,
            BattleSandboxKernelStatusTickSample sample)
        {
            if (sample == null)
            {
                report.AddError("COMBAT_KERNEL_STATUS_SAMPLE_NULL", "Status tick sample is missing.", PackageName);
                return;
            }

            ValidateEquals(report, "COMBAT_KERNEL_PLAYER_DOT", "player DOT damage per second", sample.playerDotDamagePerSecond, 5);
            ValidateEquals(report, "COMBAT_KERNEL_ENEMY_BURN_DOT", "enemy burn damage per second", sample.enemyBurnDamagePerSecond, 3);
        }

        private static void ValidateTrue(
            BuildSandboxValidationReport report,
            string code,
            bool value)
        {
            if (!value)
            {
                report.AddError(code, "Expected true for this combat kernel adapter flag.", PackageName);
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
                report.AddError(code, "Expected false for this combat kernel adapter isolation flag.", PackageName);
                return;
            }

            report.AddInfo(code, "Isolation flag remains false.", PackageName);
        }

        private static void ValidateEquals(
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

        private static void ValidateApprox(
            BuildSandboxValidationReport report,
            string code,
            string label,
            float actual,
            float expected)
        {
            if (Mathf.Abs(actual - expected) > 0.001f)
            {
                report.AddError(code, $"{label} mismatch. actual={actual:0.###}, expected={expected:0.###}.", PackageName);
                return;
            }

            report.AddInfo(code, $"{label} pass. actual={actual:0.###}.", PackageName);
        }
    }
}
#endif
