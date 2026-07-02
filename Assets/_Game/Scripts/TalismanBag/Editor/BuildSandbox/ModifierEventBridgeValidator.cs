#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TalismanBag.BuildSandbox;

namespace TalismanBag.EditorTools.BuildSandbox
{
    public static class ModifierEventBridgeValidator
    {
        private static readonly string[] RequiredModifierTypes =
        {
            ModifierEventBridge.DamageBonus,
            ModifierEventBridge.CooldownBonus,
            ModifierEventBridge.ShieldBreakBonus,
            ModifierEventBridge.ShieldBonus,
            ModifierEventBridge.CleanseBonus,
            ModifierEventBridge.ControlDurationBonus,
            ModifierEventBridge.EnergyReturnBonus
        };

        private static readonly string[] RequiredEventTypes =
        {
            ModifierEventBridge.OnBattleStart,
            ModifierEventBridge.OnShieldBreak,
            ModifierEventBridge.OnCleanse,
            ModifierEventBridge.OnShieldBroken,
            ModifierEventBridge.OnEnemyKill,
            ModifierEventBridge.OnBossSkillCast,
            ModifierEventBridge.OnLowHp,
            ModifierEventBridge.OnEnergyConnected
        };

        private static readonly string[] BridgeSourceFiles =
        {
            "Assets/_Game/Scripts/TalismanBag/BuildSandbox/ModifierEventBridge.cs"
        };

        private static readonly string[] ForbiddenFormalReferenceTokens =
        {
            "V02RunFlowController",
            "V02FormationGridFrame",
            "DamageText",
            "PageState",
            "FormationState",
            "MainTrialProgressData",
            "PlayerPrefs",
            "SaveData",
            "RewardConfig",
            "UpgradeConfig",
            "BossReward",
            "FormalDrop",
            "FormalForge"
        };

        public static BuildSandboxValidationReport Validate()
        {
            BuildSandboxValidationReport report = new("Modifier Event Bridge");
            ValidateFeatureFlags(report);
            ValidateReservedTypes(report);

            BuildSamplePreview(
                out BuildEvaluationResult buildResult,
                out AffixRarityEvaluationResult affixResult,
                out CombatModifierBundle modifierBundle,
                out EffectEventBundle eventBundle);

            ValidateInputs(report, buildResult, affixResult);
            ValidateModifierBundle(report, modifierBundle);
            ValidateEventBundle(report, eventBundle);
            ValidateSourceIsolation(report);
            return report;
        }

        public static void BuildSamplePreview(
            out BuildEvaluationResult buildResult,
            out AffixRarityEvaluationResult affixResult,
            out CombatModifierBundle modifierBundle,
            out EffectEventBundle eventBundle)
        {
            buildResult = SynergyEvaluatorCoreValidator.BuildSampleEvaluation(out _);
            affixResult = AffixRaritySandboxValidator.BuildSampleEvaluation(out _);
            modifierBundle = ModifierEventBridge.BuildCombatModifierBundle(
                buildResult,
                affixResult,
                "seed_modifier_event_bridge");
            eventBundle = ModifierEventBridge.BuildEffectEventBundle(
                buildResult,
                affixResult,
                "seed_modifier_event_bridge");
        }

        private static void ValidateFeatureFlags(BuildSandboxValidationReport report)
        {
            ValidateFalseFlag(report, nameof(BuildSandboxFeatureFlags.EnableBuildModifierInCombat), BuildSandboxFeatureFlags.EnableBuildModifierInCombat);
            ValidateFalseFlag(report, nameof(BuildSandboxFeatureFlags.EnableSynergyBuild), BuildSandboxFeatureFlags.EnableSynergyBuild);
            ValidateFalseFlag(report, nameof(BuildSandboxFeatureFlags.EnableAffixSystem), BuildSandboxFeatureFlags.EnableAffixSystem);
        }

        private static void ValidateReservedTypes(BuildSandboxValidationReport report)
        {
            foreach (string modifierType in RequiredModifierTypes)
            {
                if (ModifierEventBridge.IsSupportedModifierType(modifierType))
                {
                    report.AddInfo(
                        "MODIFIER_TYPE_RESERVED",
                        $"Reserved modifier type is supported: {modifierType}.",
                        nameof(ModifierEventBridge));
                    continue;
                }

                report.AddError(
                    "MODIFIER_TYPE_MISSING",
                    $"Required modifier type is missing: {modifierType}.",
                    nameof(ModifierEventBridge));
            }

            foreach (string eventType in RequiredEventTypes)
            {
                if (ModifierEventBridge.IsSupportedEventType(eventType))
                {
                    report.AddInfo(
                        "EVENT_TYPE_RESERVED",
                        $"Reserved event type is supported: {eventType}.",
                        nameof(ModifierEventBridge));
                    continue;
                }

                report.AddError(
                    "EVENT_TYPE_MISSING",
                    $"Required event type is missing: {eventType}.",
                    nameof(ModifierEventBridge));
            }
        }

        private static void ValidateInputs(
            BuildSandboxValidationReport report,
            BuildEvaluationResult buildResult,
            AffixRarityEvaluationResult affixResult)
        {
            if (buildResult == null)
            {
                report.AddError(
                    "BRIDGE_INPUT_BUILDEVALUATION_NULL",
                    "ModifierEventBridge requires a BuildEvaluationResult preview input.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            if (buildResult.activeSynergies.Count == 0)
            {
                report.AddError(
                    "BRIDGE_INPUT_ACTIVE_SYNERGY_MISSING",
                    "Sample BuildEvaluationResult should contain activeSynergies.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "BRIDGE_INPUT_ACTIVE_SYNERGY_PRESENT",
                    $"activeSynergies={buildResult.activeSynergies.Count}, activeThresholds={buildResult.activeThresholds.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }

            if (affixResult == null)
            {
                report.AddError(
                    "BRIDGE_INPUT_AFFIX_NULL",
                    "ModifierEventBridge requires an AffixRarityEvaluationResult preview input.",
                    BuildSandboxConfigValidator.ConfigRoot);
                return;
            }

            if (affixResult.itemResults.Count == 0 || affixResult.affixIds.Count == 0)
            {
                report.AddError(
                    "BRIDGE_INPUT_AFFIX_PREVIEW_MISSING",
                    "Sample Affix preview should contain itemResults and selected affixes.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
            else
            {
                report.AddInfo(
                    "BRIDGE_INPUT_AFFIX_PREVIEW_PRESENT",
                    $"affixItems={affixResult.itemResults.Count}, selectedAffixes={affixResult.affixIds.Count}.",
                    BuildSandboxConfigValidator.ConfigRoot);
            }
        }

        private static void ValidateModifierBundle(
            BuildSandboxValidationReport report,
            CombatModifierBundle bundle)
        {
            if (bundle == null)
            {
                report.AddError("MODIFIER_BUNDLE_NULL", "CombatModifierBundle preview is null.", nameof(ModifierEventBridge));
                return;
            }

            ValidateIsolationFlags(report, "MODIFIER_BUNDLE", bundle.devOnly, bundle.isEnabled, bundle.affectsFormalCombat);

            if (bundle.modifiers.Count == 0)
            {
                report.AddError("MODIFIER_BUNDLE_EMPTY", "CombatModifierBundle should contain preview modifiers.", nameof(ModifierEventBridge));
                return;
            }

            foreach (BuildModifierPreview modifier in bundle.modifiers)
            {
                if (!ModifierEventBridge.IsSupportedModifierType(modifier.modifierType))
                {
                    report.AddError(
                        "MODIFIER_TYPE_INVALID",
                        $"Unsupported modifier type: {modifier.modifierType}.",
                        nameof(ModifierEventBridge));
                }

                if (string.IsNullOrWhiteSpace(modifier.sourceSynergy)
                    && string.IsNullOrWhiteSpace(modifier.sourceAffix)
                    && string.IsNullOrWhiteSpace(modifier.sourceThreshold))
                {
                    report.AddError(
                        "MODIFIER_SOURCE_MISSING",
                        $"Modifier {modifier.modifierType} lacks sourceSynergy/sourceAffix/sourceThreshold.",
                        nameof(ModifierEventBridge));
                }

                ValidateIsolationFlags(report, "MODIFIER_PREVIEW", modifier.devOnly, modifier.isEnabled, modifier.affectsFormalCombat);
            }

            RequireGeneratedModifierTypes(report, bundle);

            report.AddInfo(
                "MODIFIER_BUNDLE_PREVIEW_PRESENT",
                $"CombatModifierBundle preview modifiers={bundle.modifiers.Count}.",
                nameof(ModifierEventBridge));
        }

        private static void ValidateEventBundle(
            BuildSandboxValidationReport report,
            EffectEventBundle bundle)
        {
            if (bundle == null)
            {
                report.AddError("EVENT_BUNDLE_NULL", "EffectEventBundle preview is null.", nameof(ModifierEventBridge));
                return;
            }

            ValidateIsolationFlags(report, "EVENT_BUNDLE", bundle.devOnly, bundle.isEnabled, bundle.affectsFormalCombat);

            if (bundle.events.Count == 0)
            {
                report.AddError("EVENT_BUNDLE_EMPTY", "EffectEventBundle should contain preview events.", nameof(ModifierEventBridge));
                return;
            }

            foreach (BuildEventPreview eventPreview in bundle.events)
            {
                if (!ModifierEventBridge.IsSupportedEventType(eventPreview.eventType))
                {
                    report.AddError(
                        "EVENT_TYPE_INVALID",
                        $"Unsupported event type: {eventPreview.eventType}.",
                        nameof(ModifierEventBridge));
                }

                if (string.IsNullOrWhiteSpace(eventPreview.trigger))
                {
                    report.AddError(
                        "EVENT_TRIGGER_MISSING",
                        $"Event {eventPreview.eventType} lacks trigger.",
                        nameof(ModifierEventBridge));
                }

                if (string.IsNullOrWhiteSpace(eventPreview.sourceSynergy)
                    && string.IsNullOrWhiteSpace(eventPreview.sourceAffix)
                    && string.IsNullOrWhiteSpace(eventPreview.sourceThreshold))
                {
                    report.AddError(
                        "EVENT_SOURCE_MISSING",
                        $"Event {eventPreview.eventType} lacks sourceSynergy/sourceAffix/sourceThreshold.",
                        nameof(ModifierEventBridge));
                }

                ValidateIsolationFlags(report, "EVENT_PREVIEW", eventPreview.devOnly, eventPreview.isEnabled, eventPreview.affectsFormalCombat);
            }

            RequireGeneratedEventTypes(report, bundle);

            report.AddInfo(
                "EVENT_BUNDLE_PREVIEW_PRESENT",
                $"EffectEventBundle preview events={bundle.events.Count}.",
                nameof(ModifierEventBridge));
        }

        private static void RequireGeneratedModifierTypes(
            BuildSandboxValidationReport report,
            CombatModifierBundle bundle)
        {
            HashSet<string> generatedTypes = new(
                bundle.modifiers.Select(modifier => modifier.modifierType),
                StringComparer.Ordinal);

            foreach (string modifierType in RequiredModifierTypes)
            {
                if (generatedTypes.Contains(modifierType))
                {
                    report.AddInfo(
                        "MODIFIER_TYPE_PREVIEW_GENERATED",
                        $"Sample preview generated modifier type: {modifierType}.",
                        nameof(ModifierEventBridge));
                    continue;
                }

                report.AddError(
                    "MODIFIER_TYPE_PREVIEW_MISSING",
                    $"Sample preview should generate modifier type: {modifierType}.",
                    nameof(ModifierEventBridge));
            }
        }

        private static void RequireGeneratedEventTypes(
            BuildSandboxValidationReport report,
            EffectEventBundle bundle)
        {
            HashSet<string> generatedTypes = new(
                bundle.events.Select(eventPreview => eventPreview.eventType),
                StringComparer.Ordinal);

            foreach (string eventType in RequiredEventTypes)
            {
                if (generatedTypes.Contains(eventType))
                {
                    report.AddInfo(
                        "EVENT_TYPE_PREVIEW_GENERATED",
                        $"Sample preview generated event type: {eventType}.",
                        nameof(ModifierEventBridge));
                    continue;
                }

                report.AddError(
                    "EVENT_TYPE_PREVIEW_MISSING",
                    $"Sample preview should generate event type: {eventType}.",
                    nameof(ModifierEventBridge));
            }
        }

        private static void ValidateSourceIsolation(BuildSandboxValidationReport report)
        {
            string projectRoot = Directory.GetCurrentDirectory();
            foreach (string relativePath in BridgeSourceFiles)
            {
                string path = Path.Combine(projectRoot, relativePath);
                if (!File.Exists(path))
                {
                    report.AddError(
                        "BRIDGE_SOURCE_FILE_MISSING",
                        $"Bridge source file is missing: {relativePath}.",
                        relativePath);
                    continue;
                }

                string text = File.ReadAllText(path);
                foreach (string token in ForbiddenFormalReferenceTokens)
                {
                    if (text.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        report.AddError(
                            "BRIDGE_FORMAL_REFERENCE",
                            $"Bridge source references forbidden formal system token: {token}.",
                            relativePath);
                    }
                }
            }

            report.AddInfo(
                "BRIDGE_SOURCE_ISOLATION_SCANNED",
                "ModifierEventBridge source scan completed for forbidden formal-system tokens.",
                nameof(ModifierEventBridge));
        }

        private static void ValidateFalseFlag(BuildSandboxValidationReport report, string flagName, bool value)
        {
            if (value)
            {
                report.AddError(
                    "BRIDGE_FEATURE_FLAG_TRUE",
                    $"{flagName} must stay false for ModifierEventBridge01.",
                    nameof(BuildSandboxFeatureFlags));
                return;
            }

            report.AddInfo(
                "BRIDGE_FEATURE_FLAG_FALSE",
                $"{flagName}=false.",
                nameof(BuildSandboxFeatureFlags));
        }

        private static void ValidateIsolationFlags(
            BuildSandboxValidationReport report,
            string codePrefix,
            bool devOnly,
            bool isEnabled,
            bool affectsFormalCombat)
        {
            if (!devOnly)
            {
                report.AddError($"{codePrefix}_DEVONLY_FALSE", "Preview output must keep devOnly=true.", nameof(ModifierEventBridge));
            }

            if (isEnabled)
            {
                report.AddError($"{codePrefix}_ENABLED_TRUE", "Preview output must keep isEnabled=false.", nameof(ModifierEventBridge));
            }

            if (affectsFormalCombat)
            {
                report.AddError($"{codePrefix}_AFFECTS_FORMAL_COMBAT", "Preview output must not affect formal combat.", nameof(ModifierEventBridge));
            }
        }
    }
}
#endif
