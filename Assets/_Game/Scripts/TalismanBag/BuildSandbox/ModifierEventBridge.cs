using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class CombatModifierBundle
    {
        public string sourceBuildId = "build_sandbox_preview";
        public bool devOnly = true;
        public bool isEnabled;
        public bool affectsFormalCombat;
        public List<BuildModifierPreview> modifiers = new();

        public int Count => modifiers.Count;
    }

    [Serializable]
    public sealed class EffectEventBundle
    {
        public string sourceBuildId = "build_sandbox_preview";
        public bool devOnly = true;
        public bool isEnabled;
        public bool affectsFormalCombat;
        public List<BuildEventPreview> events = new();

        public int Count => events.Count;
    }

    [Serializable]
    public sealed class BuildModifierPreview
    {
        public string modifierType = string.Empty;
        public float previewValue;
        public string sourceSynergy = string.Empty;
        public string sourceThreshold = string.Empty;
        public string sourceAffix = string.Empty;
        public string sourceItem = string.Empty;
        public string previewSummary = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool affectsFormalCombat;
    }

    [Serializable]
    public sealed class BuildEventPreview
    {
        public string eventType = string.Empty;
        public string trigger = string.Empty;
        public string sourceSynergy = string.Empty;
        public string sourceThreshold = string.Empty;
        public string sourceAffix = string.Empty;
        public string sourceItem = string.Empty;
        public string previewPayload = string.Empty;
        public bool devOnly = true;
        public bool isEnabled;
        public bool affectsFormalCombat;
    }

    public static class ModifierEventBridge
    {
        public const string DamageBonus = "damageBonus";
        public const string CooldownBonus = "cooldownBonus";
        public const string ShieldBreakBonus = "shieldBreakBonus";
        public const string ShieldBonus = "shieldBonus";
        public const string CleanseBonus = "cleanseBonus";
        public const string ControlDurationBonus = "controlDurationBonus";
        public const string EnergyReturnBonus = "energyReturnBonus";

        public const string OnBattleStart = "onBattleStart";
        public const string OnShieldBreak = "onShieldBreak";
        public const string OnCleanse = "onCleanse";
        public const string OnShieldBroken = "onShieldBroken";
        public const string OnEnemyKill = "onEnemyKill";
        public const string OnBossSkillCast = "onBossSkillCast";
        public const string OnLowHp = "onLowHp";
        public const string OnEnergyConnected = "onEnergyConnected";

        public static readonly string[] SupportedModifierTypes =
        {
            DamageBonus,
            CooldownBonus,
            ShieldBreakBonus,
            ShieldBonus,
            CleanseBonus,
            ControlDurationBonus,
            EnergyReturnBonus
        };

        public static readonly string[] SupportedEventTypes =
        {
            OnBattleStart,
            OnShieldBreak,
            OnCleanse,
            OnShieldBroken,
            OnEnemyKill,
            OnBossSkillCast,
            OnLowHp,
            OnEnergyConnected
        };

        public static CombatModifierBundle BuildCombatModifierBundle(
            BuildEvaluationResult buildResult,
            AffixRarityEvaluationResult affixResult = null,
            string sourceBuildId = "seed_modifier_event_bridge")
        {
            CombatModifierBundle bundle = new()
            {
                sourceBuildId = NormalizeSourceBuildId(sourceBuildId),
                devOnly = true,
                isEnabled = false,
                affectsFormalCombat = false
            };

            AddSynergyModifiers(bundle, buildResult);
            AddAffixModifiers(bundle, affixResult);
            return bundle;
        }

        public static EffectEventBundle BuildEffectEventBundle(
            BuildEvaluationResult buildResult,
            AffixRarityEvaluationResult affixResult = null,
            string sourceBuildId = "seed_modifier_event_bridge")
        {
            EffectEventBundle bundle = new()
            {
                sourceBuildId = NormalizeSourceBuildId(sourceBuildId),
                devOnly = true,
                isEnabled = false,
                affectsFormalCombat = false
            };

            AddSynergyEvents(bundle, buildResult);
            AddAffixEvents(bundle, affixResult);
            return bundle;
        }

        public static bool IsSupportedModifierType(string modifierType)
        {
            return SupportedModifierTypes.Contains(modifierType ?? string.Empty, StringComparer.Ordinal);
        }

        public static bool IsSupportedEventType(string eventType)
        {
            return SupportedEventTypes.Contains(eventType ?? string.Empty, StringComparer.Ordinal);
        }

        private static void AddSynergyModifiers(CombatModifierBundle bundle, BuildEvaluationResult buildResult)
        {
            foreach (ActiveSynergyResult synergy in buildResult?.activeSynergies ?? Enumerable.Empty<ActiveSynergyResult>())
            {
                if (synergy == null)
                {
                    continue;
                }

                foreach (string thresholdKey in synergy.activeThresholds ?? Enumerable.Empty<string>())
                {
                    int pieceCount = ParseThresholdPieceCount(thresholdKey);
                    float tierValue = Math.Max(1, pieceCount) * 0.05f;
                    AddModifier(
                        bundle,
                        DamageBonus,
                        tierValue,
                        synergy.synergyId,
                        thresholdKey,
                        string.Empty,
                        FormatStrings(synergy.sourceItems),
                        $"Preview damage tier from {synergy.synergyId} threshold {thresholdKey}.");

                    if (pieceCount >= 4)
                    {
                        AddModifier(
                            bundle,
                            CooldownBonus,
                            tierValue * 0.5f,
                            synergy.synergyId,
                            thresholdKey,
                            string.Empty,
                            FormatStrings(synergy.sourceItems),
                            $"Preview cooldown tier from {synergy.synergyId} threshold {thresholdKey}.");
                    }
                }

                if (synergy.placementSatisfied)
                {
                    AddModifier(
                        bundle,
                        ShieldBreakBonus,
                        0.08f,
                        synergy.synergyId,
                        FormatStrings(synergy.activeThresholds),
                        string.Empty,
                        FormatStrings(synergy.sourceItems),
                        $"Preview placement-based shield break bonus from {synergy.synergyId}.");
                }

                if (synergy.energySatisfied)
                {
                    AddModifier(
                        bundle,
                        EnergyReturnBonus,
                        0.06f,
                        synergy.synergyId,
                        FormatStrings(synergy.activeThresholds),
                        string.Empty,
                        FormatStrings(synergy.sourceItems),
                        $"Preview energy return bonus from {synergy.synergyId}.");
                }
            }
        }

        private static void AddAffixModifiers(CombatModifierBundle bundle, AffixRarityEvaluationResult affixResult)
        {
            foreach (AffixRarityItemResult item in affixResult?.itemResults ?? Enumerable.Empty<AffixRarityItemResult>())
            {
                if (item == null)
                {
                    continue;
                }

                float previewValue = Math.Max(1, item.totalPreviewPower) / 100f;
                foreach (string affixId in item.selectedAffixes ?? Enumerable.Empty<string>())
                {
                    string modifierType = ResolveModifierTypeForAffix(affixId, item);
                    AddModifier(
                        bundle,
                        modifierType,
                        previewValue,
                        string.Empty,
                        string.Empty,
                        affixId,
                        item.itemId,
                        $"Preview {modifierType} from affix {affixId} on {item.itemId}.");
                }

                if (HasTag(item.sourceTags, "control_preview") && item.selectedAffixes.Count > 0)
                {
                    AddModifier(
                        bundle,
                        ControlDurationBonus,
                        Math.Max(0.05f, previewValue * 0.5f),
                        string.Empty,
                        string.Empty,
                        FormatStrings(item.selectedAffixes),
                        item.itemId,
                        $"Preview control duration bonus from {item.itemId} control tags.");
                }
            }
        }

        private static void AddSynergyEvents(EffectEventBundle bundle, BuildEvaluationResult buildResult)
        {
            foreach (ActiveSynergyResult synergy in buildResult?.activeSynergies ?? Enumerable.Empty<ActiveSynergyResult>())
            {
                if (synergy == null)
                {
                    continue;
                }

                foreach (string thresholdKey in synergy.activeThresholds ?? Enumerable.Empty<string>())
                {
                    AddEvent(
                        bundle,
                        OnBattleStart,
                        OnBattleStart,
                        synergy.synergyId,
                        thresholdKey,
                        string.Empty,
                        FormatStrings(synergy.sourceItems),
                        $"Preview battle-start event from {synergy.synergyId} threshold {thresholdKey}.");

                    if (ParseThresholdPieceCount(thresholdKey) >= 4)
                    {
                        AddEvent(
                            bundle,
                            OnShieldBreak,
                            OnShieldBreak,
                            synergy.synergyId,
                            thresholdKey,
                            string.Empty,
                            FormatStrings(synergy.sourceItems),
                            $"Preview shield-break event from {synergy.synergyId} threshold {thresholdKey}.");
                    }
                }

                if (synergy.energySatisfied)
                {
                    AddEvent(
                        bundle,
                        OnEnergyConnected,
                        OnEnergyConnected,
                        synergy.synergyId,
                        FormatStrings(synergy.activeThresholds),
                        string.Empty,
                        FormatStrings(synergy.sourceItems),
                        $"Preview energy-connected event from {synergy.synergyId}.");
                }
            }
        }

        private static void AddAffixEvents(EffectEventBundle bundle, AffixRarityEvaluationResult affixResult)
        {
            foreach (AffixRarityItemResult item in affixResult?.itemResults ?? Enumerable.Empty<AffixRarityItemResult>())
            {
                if (item == null)
                {
                    continue;
                }

                foreach (string affixId in item.selectedAffixes ?? Enumerable.Empty<string>())
                {
                    string eventType = ResolveEventTypeForAffix(affixId, item);
                    AddEvent(
                        bundle,
                        eventType,
                        eventType,
                        string.Empty,
                        string.Empty,
                        affixId,
                        item.itemId,
                        $"Preview {eventType} from affix {affixId} on {item.itemId}.");

                    if (ContainsToken(affixId, "lihuo") || HasTag(item.sourceTags, "damage_preview"))
                    {
                        AddEvent(
                            bundle,
                            OnEnemyKill,
                            OnEnemyKill,
                            string.Empty,
                            string.Empty,
                            affixId,
                            item.itemId,
                            $"Preview enemy-kill event from damage affix {affixId}.");
                    }
                }

                if (HasTag(item.sourceTags, "defense_preview") || HasTag(item.sourceTags, "ward_preview"))
                {
                    AddEvent(
                        bundle,
                        OnLowHp,
                        OnLowHp,
                        string.Empty,
                        string.Empty,
                        FormatStrings(item.selectedAffixes),
                        item.itemId,
                        $"Preview low-hp defensive event from {item.itemId}.");
                }
            }
        }

        private static void AddModifier(
            CombatModifierBundle bundle,
            string modifierType,
            float previewValue,
            string sourceSynergy,
            string sourceThreshold,
            string sourceAffix,
            string sourceItem,
            string previewSummary)
        {
            if (!IsSupportedModifierType(modifierType))
            {
                return;
            }

            bundle.modifiers.Add(new BuildModifierPreview
            {
                modifierType = modifierType,
                previewValue = previewValue,
                sourceSynergy = sourceSynergy ?? string.Empty,
                sourceThreshold = sourceThreshold ?? string.Empty,
                sourceAffix = sourceAffix ?? string.Empty,
                sourceItem = sourceItem ?? string.Empty,
                previewSummary = previewSummary ?? string.Empty,
                devOnly = true,
                isEnabled = false,
                affectsFormalCombat = false
            });
        }

        private static void AddEvent(
            EffectEventBundle bundle,
            string eventType,
            string trigger,
            string sourceSynergy,
            string sourceThreshold,
            string sourceAffix,
            string sourceItem,
            string previewPayload)
        {
            if (!IsSupportedEventType(eventType))
            {
                return;
            }

            bundle.events.Add(new BuildEventPreview
            {
                eventType = eventType,
                trigger = trigger ?? string.Empty,
                sourceSynergy = sourceSynergy ?? string.Empty,
                sourceThreshold = sourceThreshold ?? string.Empty,
                sourceAffix = sourceAffix ?? string.Empty,
                sourceItem = sourceItem ?? string.Empty,
                previewPayload = previewPayload ?? string.Empty,
                devOnly = true,
                isEnabled = false,
                affectsFormalCombat = false
            });
        }

        private static string ResolveModifierTypeForAffix(string affixId, AffixRarityItemResult item)
        {
            if (ContainsToken(affixId, "orange_core") || ContainsToken(affixId, "core"))
            {
                return ShieldBreakBonus;
            }

            if (ContainsToken(affixId, "bond"))
            {
                return CooldownBonus;
            }

            if (ContainsToken(affixId, "lihuo") || ContainsToken(affixId, "spark"))
            {
                return DamageBonus;
            }

            if (ContainsToken(affixId, "guardian") || ContainsToken(affixId, "ward"))
            {
                return ShieldBonus;
            }

            if (ContainsToken(affixId, "purifying") || ContainsToken(affixId, "seal"))
            {
                return CleanseBonus;
            }

            if (ContainsToken(affixId, "focus") || ContainsToken(affixId, "gather"))
            {
                return EnergyReturnBonus;
            }

            if (HasTag(item?.sourceTags, "damage_preview"))
            {
                return DamageBonus;
            }

            if (HasTag(item?.sourceTags, "defense_preview"))
            {
                return ShieldBonus;
            }

            if (HasTag(item?.sourceTags, "cleanse_preview"))
            {
                return CleanseBonus;
            }

            if (HasTag(item?.sourceTags, "energy_preview"))
            {
                return EnergyReturnBonus;
            }

            if (HasTag(item?.sourceTags, "control_preview"))
            {
                return ControlDurationBonus;
            }

            return DamageBonus;
        }

        private static string ResolveEventTypeForAffix(string affixId, AffixRarityItemResult item)
        {
            if (ContainsToken(affixId, "orange_core") || ContainsToken(affixId, "core"))
            {
                return OnBossSkillCast;
            }

            if (ContainsToken(affixId, "purifying") || ContainsToken(affixId, "seal"))
            {
                return OnCleanse;
            }

            if (ContainsToken(affixId, "guardian") || ContainsToken(affixId, "ward"))
            {
                return OnShieldBroken;
            }

            if (ContainsToken(affixId, "focus") || ContainsToken(affixId, "gather"))
            {
                return OnEnergyConnected;
            }

            if (ContainsToken(affixId, "lihuo") || ContainsToken(affixId, "spark"))
            {
                return OnShieldBreak;
            }

            if (HasTag(item?.sourceTags, "cleanse_preview"))
            {
                return OnCleanse;
            }

            if (HasTag(item?.sourceTags, "energy_preview"))
            {
                return OnEnergyConnected;
            }

            if (HasTag(item?.sourceTags, "damage_preview"))
            {
                return OnShieldBreak;
            }

            return OnBattleStart;
        }

        private static int ParseThresholdPieceCount(string thresholdKey)
        {
            if (string.IsNullOrWhiteSpace(thresholdKey))
            {
                return 0;
            }

            int index = thresholdKey.LastIndexOf(':');
            if (index < 0 || index >= thresholdKey.Length - 1)
            {
                return 0;
            }

            return int.TryParse(thresholdKey.Substring(index + 1), out int pieceCount)
                ? pieceCount
                : 0;
        }

        private static bool HasTag(IEnumerable<string> tags, string tag)
        {
            return (tags ?? Enumerable.Empty<string>()).Contains(tag ?? string.Empty, StringComparer.Ordinal);
        }

        private static bool ContainsToken(string value, string token)
        {
            return !string.IsNullOrWhiteSpace(value)
                && !string.IsNullOrWhiteSpace(token)
                && value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string NormalizeSourceBuildId(string sourceBuildId)
        {
            return string.IsNullOrWhiteSpace(sourceBuildId)
                ? "seed_modifier_event_bridge"
                : sourceBuildId.Trim();
        }

        private static string FormatStrings(IEnumerable<string> values)
        {
            return string.Join(";", (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)));
        }
    }
}
