using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class LegacyItemBehaviorProfile
    {
        public string itemId = string.Empty;
        public string chineseName = string.Empty;
        public string englishStableKey = string.Empty;
        public string itemFamily = string.Empty;
        public string baseItemId = string.Empty;
        public string advancedItemId = string.Empty;
        public string advancedItemChineseName = string.Empty;
        public string advancedRelationshipChinese = string.Empty;
        public string behaviorSummaryChinese = string.Empty;
        public bool requiredCoreItem;
        public bool dealsEnemyDamage;
        public bool breaksShield;
        public bool cleansesNegative;
        public bool controlsEnemy;
        public bool grantsShield;
        public bool restoresMana;
        public bool formalEnergyProvider;
        public bool auxiliaryNonProvider;
        public bool devOnly = true;
        public bool isEnabled;
    }

    [Serializable]
    public sealed class LegacyItemBehaviorResult
    {
        public string itemId = string.Empty;
        public string chineseName = string.Empty;
        public string englishStableKey = string.Empty;
        public string itemFamily = string.Empty;
        public string baseItemId = string.Empty;
        public string advancedItemId = string.Empty;
        public string advancedItemChineseName = string.Empty;
        public string advancedRelationshipChinese = string.Empty;
        public EnergyState energyState;
        public bool triggers;
        public bool allowAffixSynergyBonus;
        public int efficiencyPercent;
        public int manaCost;
        public int enemyHpDamage;
        public int enemyShieldPressure;
        public int playerShieldGain;
        public int cleanseValue;
        public int controlValue;
        public int manaGain;
        public string playerFeedbackChinese = string.Empty;
        public string combatLogChinese = string.Empty;
        public string sampleResultChinese = string.Empty;
        public string sourceDataPath = LegacyItemBehaviorIntegrationCatalog.SourceDataPath;
        public bool developerFieldsHidden = true;
        public bool touchesFormalFlow;
        public bool touchesFormalSaveData;
        public bool touchesFormalReward;
        public bool touchesFormalDrop;
        public bool devOnly = true;
        public bool isEnabled;

        public bool HasAnyOutput =>
            enemyHpDamage > 0
            || enemyShieldPressure > 0
            || playerShieldGain > 0
            || cleanseValue > 0
            || controlValue > 0
            || manaGain > 0;

        public int FormalLeakCount =>
            (touchesFormalFlow ? 1 : 0)
            + (touchesFormalSaveData ? 1 : 0)
            + (touchesFormalReward ? 1 : 0)
            + (touchesFormalDrop ? 1 : 0);
    }

    [Serializable]
    public sealed class LegacyItemBehaviorSampleRow
    {
        public string itemId = string.Empty;
        public string chineseName = string.Empty;
        public string englishStableKey = string.Empty;
        public string itemFamily = string.Empty;
        public string baseItemId = string.Empty;
        public string advancedItemId = string.Empty;
        public string advancedItemChineseName = string.Empty;
        public string advancedRelationshipChinese = string.Empty;
        public EnergyState energyState;
        public bool triggers;
        public bool allowAffixSynergyBonus;
        public int efficiencyPercent;
        public int manaCost;
        public int enemyHpDamage;
        public int enemyShieldPressure;
        public int playerShieldGain;
        public int cleanseValue;
        public int controlValue;
        public int manaGain;
        public string playerFeedbackChinese = string.Empty;
        public string combatLogChinese = string.Empty;
        public string sampleResultChinese = string.Empty;
        public bool developerFieldsHidden = true;
        public bool touchesFormalFlow;
        public bool touchesFormalSaveData;
        public bool touchesFormalReward;
        public bool touchesFormalDrop;
        public bool devOnly = true;
        public bool isEnabled;
        public string sourceDataPath = LegacyItemBehaviorIntegrationCatalog.SourceDataPath;

        public bool HasAnyOutput =>
            enemyHpDamage > 0
            || enemyShieldPressure > 0
            || playerShieldGain > 0
            || cleanseValue > 0
            || controlValue > 0
            || manaGain > 0;

        public int FormalLeakCount =>
            (touchesFormalFlow ? 1 : 0)
            + (touchesFormalSaveData ? 1 : 0)
            + (touchesFormalReward ? 1 : 0)
            + (touchesFormalDrop ? 1 : 0);
    }

    [Serializable]
    public sealed class LegacyItemFamilyBehaviorMapRow
    {
        public string itemFamily = string.Empty;
        public string baseItemId = string.Empty;
        public string baseChineseName = string.Empty;
        public string englishStableKey = string.Empty;
        public string v04AdvancedItemId = string.Empty;
        public string v04AdvancedChineseName = string.Empty;
        public string advancedRelationshipChinese = string.Empty;
        public string behaviorSummaryChinese = string.Empty;
        public bool baseItemPresentInRoster;
        public bool advancedItemPresentInRoster;
        public bool formalEnergyProvider;
        public bool auxiliaryNonProvider;
        public bool devOnly = true;
        public bool isEnabled;
    }

    [Serializable]
    public sealed class LegacyItemBehaviorIntegrationPreview
    {
        public string packageName = LegacyItemBehaviorIntegrationCatalog.PackageName;
        public string guardPass = LegacyItemBehaviorIntegrationCatalog.GuardPass;
        public bool devOnly = true;
        public bool isEnabled;
        public bool readsEnergyState = true;
        public bool reusesFormationEnergyContract = true;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool writesFormalReward;
        public bool writesFormalDrop;
        public bool touchesFormalBackpack;
        public int rosterItemCount;
        public int behaviorProfileCount;
        public int requiredCoreProfileCount;
        public int requiredCorePoweredOutputCount;
        public int energyStateSampleRowCount;
        public int familyMapRowCount;
        public int noneOutputLeakCount;
        public int weakPulseFullPowerLeakCount;
        public int poweredMissingOutputCount;
        public int suppressedOutputLeakCount;
        public int energyProviderDamageLeakCount;
        public int forbiddenProviderLeakCount;
        public int playerSideAnswerLeakCount;
        public int developerFieldVisibleLeakCount;
        public int formalLeakCount;
        public int featureFlagDefaultTrueCount;
        public List<LegacyItemBehaviorSampleRow> rows = new();
        public List<LegacyItemFamilyBehaviorMapRow> familyRows = new();

        public int ScopeLeakCount =>
            (writesFormalFlow ? 1 : 0)
            + (writesFormalSaveData ? 1 : 0)
            + (writesFormalReward ? 1 : 0)
            + (writesFormalDrop ? 1 : 0)
            + (touchesFormalBackpack ? 1 : 0)
            + formalLeakCount;

        public int TotalLeakCount =>
            noneOutputLeakCount
            + weakPulseFullPowerLeakCount
            + poweredMissingOutputCount
            + suppressedOutputLeakCount
            + energyProviderDamageLeakCount
            + forbiddenProviderLeakCount
            + playerSideAnswerLeakCount
            + developerFieldVisibleLeakCount
            + ScopeLeakCount
            + featureFlagDefaultTrueCount;

        public bool Passed =>
            devOnly
            && !isEnabled
            && readsEnergyState
            && reusesFormationEnergyContract
            && rosterItemCount > 0
            && behaviorProfileCount >= rosterItemCount
            && requiredCoreProfileCount >= LegacyItemBehaviorIntegrationCatalog.RequiredCoreBaseItemIds.Count
            && requiredCorePoweredOutputCount >= LegacyItemBehaviorIntegrationCatalog.RequiredCoreBaseItemIds.Count
            && energyStateSampleRowCount >= behaviorProfileCount * 4
            && familyMapRowCount > 0
            && TotalLeakCount == 0;
    }

    public static class LegacyItemBehaviorIntegrationCatalog
    {
        public const string PackageName = "V0.4-LegacyItemBehaviorIntegration01";
        public const string GuardPass = "GUARD_PASS_LEGACY_ITEM_BEHAVIOR_INTEGRATION01";
        public const string SourceDataPath = "LegacyItemBehaviorIntegrationCatalog";
        public const string DeveloperDataPanelFieldKey = "legacyItemBehaviorIntegration";

        public static readonly IReadOnlyList<string> RequiredCoreBaseItemIds = new[]
        {
            "fire_talisman_basic",
            "thunder_talisman_basic",
            "exorcism_bell_basic",
            "purify_talisman_basic",
            "soul_suppress_talisman_basic",
            "peach_wood_basic",
            "spirit_stone_basic"
        };

        private static readonly Dictionary<string, LegacyItemBehaviorProfile> ProfilesByItemId =
            BuildProfiles().ToDictionary(profile => profile.itemId, StringComparer.Ordinal);

        public static IReadOnlyList<LegacyItemBehaviorProfile> AllProfiles =>
            ProfilesByItemId.Values.ToArray();

        public static LegacyItemBehaviorProfile Resolve(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat = null)
        {
            if (item != null
                && ProfilesByItemId.TryGetValue(item.itemId ?? string.Empty, out LegacyItemBehaviorProfile profile))
            {
                return profile;
            }

            return Resolve(item?.itemId, item?.itemFamily, item?.baseItemId, stat);
        }

        public static LegacyItemBehaviorProfile Resolve(
            string itemId,
            string itemFamily = "",
            string baseItemId = "",
            BuildSandboxItemStat stat = null)
        {
            string safeItemId = itemId ?? string.Empty;
            if (ProfilesByItemId.TryGetValue(safeItemId, out LegacyItemBehaviorProfile profile))
            {
                return profile;
            }

            BuildSandboxItemIdentityFamilyRecord identity =
                BuildSandboxItemIdentityFamilyCatalog.Resolve(safeItemId);
            string family = string.IsNullOrWhiteSpace(itemFamily) ? identity.ItemFamily : itemFamily;
            string baseId = string.IsNullOrWhiteSpace(baseItemId) ? identity.BaseItemId : baseItemId;
            BattleSandboxItemEffectRuntimeProfile runtimeProfile =
                BattleSandboxItemEffectRuntimePreviewCatalog.Resolve(safeItemId, family, stat?.statTag);
            return new LegacyItemBehaviorProfile
            {
                itemId = safeItemId,
                chineseName = string.IsNullOrWhiteSpace(identity.DisplayName)
                    ? runtimeProfile.displayNameChinese
                    : identity.DisplayName,
                englishStableKey = NormalizeStableKey(safeItemId),
                itemFamily = family,
                baseItemId = baseId,
                advancedRelationshipChinese = identity.RelationshipToBase,
                behaviorSummaryChinese = "沙盒兜底行为反馈，仅用于观察。",
                dealsEnemyDamage = runtimeProfile.dealsEnemyDamage,
                breaksShield = runtimeProfile.breaksShield,
                cleansesNegative = runtimeProfile.cleanses,
                controlsEnemy = runtimeProfile.controlsBoss,
                grantsShield = runtimeProfile.grantsShield,
                restoresMana = runtimeProfile.restoresMana && IsSpiritStoneIdentity(safeItemId, family, baseId),
                formalEnergyProvider = IsSpiritStoneIdentity(safeItemId, family, baseId),
                auxiliaryNonProvider = IsAuxiliaryNonProvider(safeItemId, family)
            };
        }

        public static LegacyItemBehaviorResult Evaluate(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat = null)
        {
            BuildSandboxItemStat safeStat = stat ?? BuildSandboxItemStatCatalog.ResolveFrom(item?.itemStat, item?.itemId);
            LegacyItemBehaviorProfile profile = Resolve(item, safeStat);
            EnergyState state = item?.energyState ?? EnergyState.None;
            int efficiency = ResolveEfficiencyPercent(state);
            bool triggers = efficiency > 0;
            bool allowAffixSynergy = triggers
                && state == EnergyState.Powered
                && !profile.formalEnergyProvider
                && !profile.auxiliaryNonProvider;

            int attackDamage = profile.dealsEnemyDamage
                ? Mathf.Max(0, safeStat.attack) * 3
                    + Mathf.Max(0, safeStat.shieldBreak)
                    + (state == EnergyState.Powered ? Mathf.Max(0, safeStat.spirit) : 0)
                : 0;
            int shieldPressure = profile.breaksShield
                ? Mathf.Max(0, safeStat.shieldBreak + safeStat.attack / 2)
                : 0;
            int shieldGain = profile.grantsShield
                ? Mathf.Max(0, safeStat.guard * 2 + (state == EnergyState.Powered ? safeStat.spirit : 0))
                : 0;
            int cleanse = profile.cleansesNegative
                ? Mathf.Max(0, safeStat.cleanse)
                : 0;
            int control = profile.controlsEnemy
                ? Mathf.Max(0, safeStat.control + safeStat.cleanse / 2)
                : 0;
            int manaGain = profile.formalEnergyProvider && state == EnergyState.Powered
                ? Mathf.Max(0, safeStat.manaGainPerTick)
                : 0;

            LegacyItemBehaviorResult result = new()
            {
                itemId = profile.itemId,
                chineseName = profile.chineseName,
                englishStableKey = profile.englishStableKey,
                itemFamily = profile.itemFamily,
                baseItemId = profile.baseItemId,
                advancedItemId = profile.advancedItemId,
                advancedItemChineseName = profile.advancedItemChineseName,
                advancedRelationshipChinese = profile.advancedRelationshipChinese,
                energyState = state,
                triggers = triggers,
                allowAffixSynergyBonus = allowAffixSynergy,
                efficiencyPercent = efficiency,
                manaCost = triggers ? Mathf.Max(0, safeStat.manaCostPerCast) : 0,
                enemyHpDamage = Scale(attackDamage, efficiency),
                enemyShieldPressure = Scale(shieldPressure, efficiency),
                playerShieldGain = Scale(shieldGain, efficiency),
                cleanseValue = Scale(cleanse, efficiency),
                controlValue = Scale(control, efficiency),
                manaGain = manaGain,
                playerFeedbackChinese = BuildPlayerFeedback(profile, state, efficiency),
                combatLogChinese = BuildCombatLog(profile, state, efficiency),
                sourceDataPath = $"{SourceDataPath}.{profile.englishStableKey}.{state}",
                developerFieldsHidden = true,
                devOnly = true,
                isEnabled = false
            };
            result.sampleResultChinese = BuildSampleResult(result);
            return result;
        }

        public static LegacyItemBehaviorSampleRow ToSampleRow(LegacyItemBehaviorResult result)
        {
            LegacyItemBehaviorResult safe = result ?? new LegacyItemBehaviorResult();
            return new LegacyItemBehaviorSampleRow
            {
                itemId = safe.itemId,
                chineseName = safe.chineseName,
                englishStableKey = safe.englishStableKey,
                itemFamily = safe.itemFamily,
                baseItemId = safe.baseItemId,
                advancedItemId = safe.advancedItemId,
                advancedItemChineseName = safe.advancedItemChineseName,
                advancedRelationshipChinese = safe.advancedRelationshipChinese,
                energyState = safe.energyState,
                triggers = safe.triggers,
                allowAffixSynergyBonus = safe.allowAffixSynergyBonus,
                efficiencyPercent = safe.efficiencyPercent,
                manaCost = safe.manaCost,
                enemyHpDamage = safe.enemyHpDamage,
                enemyShieldPressure = safe.enemyShieldPressure,
                playerShieldGain = safe.playerShieldGain,
                cleanseValue = safe.cleanseValue,
                controlValue = safe.controlValue,
                manaGain = safe.manaGain,
                playerFeedbackChinese = safe.playerFeedbackChinese,
                combatLogChinese = safe.combatLogChinese,
                sampleResultChinese = safe.sampleResultChinese,
                developerFieldsHidden = safe.developerFieldsHidden,
                touchesFormalFlow = safe.touchesFormalFlow,
                touchesFormalSaveData = safe.touchesFormalSaveData,
                touchesFormalReward = safe.touchesFormalReward,
                touchesFormalDrop = safe.touchesFormalDrop,
                devOnly = safe.devOnly,
                isEnabled = safe.isEnabled,
                sourceDataPath = safe.sourceDataPath
            };
        }

        public static IReadOnlyList<LegacyItemFamilyBehaviorMapRow> BuildFamilyRows(
            IReadOnlyList<BuildSandboxLegacyAndAdvancedItemRosterRow> roster)
        {
            HashSet<string> rosterIds = new(
                (roster ?? Array.Empty<BuildSandboxLegacyAndAdvancedItemRosterRow>())
                .Where(row => row != null)
                .Select(row => row.ItemId),
                StringComparer.Ordinal);
            return AllProfiles
                .GroupBy(profile => string.IsNullOrWhiteSpace(profile.baseItemId)
                    ? profile.itemId
                    : profile.baseItemId,
                    StringComparer.Ordinal)
                .Select(group =>
                {
                    LegacyItemBehaviorProfile profile =
                        group.FirstOrDefault(row => string.Equals(row.itemId, row.baseItemId, StringComparison.Ordinal))
                        ?? group.First();
                    return new LegacyItemFamilyBehaviorMapRow
                    {
                        itemFamily = profile.itemFamily,
                        baseItemId = profile.baseItemId,
                        baseChineseName = profile.chineseName,
                        englishStableKey = profile.englishStableKey,
                        v04AdvancedItemId = profile.advancedItemId,
                        v04AdvancedChineseName = profile.advancedItemChineseName,
                        advancedRelationshipChinese = profile.advancedRelationshipChinese,
                        behaviorSummaryChinese = profile.behaviorSummaryChinese,
                        baseItemPresentInRoster = string.IsNullOrWhiteSpace(profile.baseItemId)
                            || rosterIds.Contains(profile.baseItemId),
                        advancedItemPresentInRoster = string.IsNullOrWhiteSpace(profile.advancedItemId)
                            || rosterIds.Contains(profile.advancedItemId),
                        formalEnergyProvider = profile.formalEnergyProvider,
                        auxiliaryNonProvider = profile.auxiliaryNonProvider,
                        devOnly = true,
                        isEnabled = false
                    };
                })
                .OrderBy(row => row.itemFamily, StringComparer.Ordinal)
                .ThenBy(row => row.baseItemId, StringComparer.Ordinal)
                .ToArray();
        }

        public static bool IsForbiddenPoweredProviderCandidate(string itemId)
        {
            string safe = itemId ?? string.Empty;
            return string.Equals(safe, "preview_energy_incense", StringComparison.Ordinal)
                || string.Equals(safe, "preview_stone_core", StringComparison.Ordinal);
        }

        private static int ResolveEfficiencyPercent(EnergyState state)
        {
            switch (state)
            {
                case EnergyState.WeakPulse:
                    return 45;
                case EnergyState.Powered:
                    return 100;
                case EnergyState.None:
                case EnergyState.Suppressed:
                default:
                    return 0;
            }
        }

        private static int Scale(int value, int efficiencyPercent)
        {
            if (value <= 0 || efficiencyPercent <= 0)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(value * efficiencyPercent / 100f));
        }

        private static IEnumerable<LegacyItemBehaviorProfile> BuildProfiles()
        {
            yield return Profile("fire_talisman_basic", "火符", "legacy_fire_talisman", "fire_talisman", "fire_talisman_basic", "preview_fire_talisman", "炽火符", "火符 family 的进阶 / 分支", "基础火伤，并保留持续灼烧倾向。", required: true, damage: true);
            yield return Profile("preview_fire_talisman", "炽火符", "advanced_fire_talisman", "fire_talisman", "fire_talisman_basic", "preview_fire_talisman", "炽火符", "火符 family 的进阶 / 分支", "炽火符扩展火符基础火伤节奏。", damage: true);

            yield return Profile("thunder_talisman_basic", "雷符", "legacy_thunder_talisman", "thunder_talisman", "thunder_talisman_basic", "preview_thunder_sword", "雷引剑符", "雷符 family 的进阶 / 分支", "雷击、短促爆发，并带破盾倾向。", required: true, damage: true, breakShield: true);
            yield return Profile("chain_thunder_talisman_basic", "连锁雷符", "legacy_chain_thunder_talisman", "chain_thunder_talisman", "chain_thunder_talisman_basic", "preview_thunder_sword", "雷引剑符", "雷符 family 的连锁旧件 / 分支参考", "连锁雷符保留雷击扩散与破盾倾向。", damage: true, breakShield: true);
            yield return Profile("preview_thunder_sword", "雷引剑符", "advanced_thunder_sword", "thunder_talisman", "thunder_talisman_basic", "preview_thunder_sword", "雷引剑符", "雷符 family 的进阶 / 分支", "雷引剑符扩展雷符破盾爆发。", damage: true, breakShield: true);

            yield return Profile("exorcism_bell_basic", "驱邪铃", "legacy_exorcism_bell", "exorcism_bell", "exorcism_bell_basic", "preview_old_bell", "镇邪铃", "驱邪铃 family 的进阶 / 分支", "对邪祟、污染和负面机制有克制反馈。", required: true, damage: true, control: true, cleanse: true);
            yield return Profile("preview_old_bell", "镇邪铃", "advanced_old_bell", "exorcism_bell", "exorcism_bell_basic", "preview_old_bell", "镇邪铃", "驱邪铃 family 的进阶 / 分支", "镇邪铃扩展驱邪铃的镇邪压制。", control: true, cleanse: true);

            yield return Profile("purify_talisman_basic", "净化符", "legacy_purify_talisman", "purify_talisman", "purify_talisman_basic", "preview_cleanse_corner", "净化折符", "净化符 family 的进阶 / 分支", "清除或降低负面状态。", required: true, cleanse: true);
            yield return Profile("preview_cleanse_corner", "净化折符", "advanced_cleanse_corner", "purify_talisman", "purify_talisman_basic", "preview_cleanse_corner", "净化折符", "净化符 family 的进阶 / 分支", "净化折符扩展净化符的形状与净化反馈。", cleanse: true);

            yield return Profile("soul_suppress_talisman_basic", "镇魂符", "legacy_soul_suppress_talisman", "soul_suppress_talisman", "soul_suppress_talisman_basic", "preview_soul_seal", "镇魂法印", "镇魂符 family 的进阶 / 分支", "控制、稳定并压制敌方行动倾向。", required: true, control: true, cleanse: true);
            yield return Profile("preview_soul_seal", "镇魂法印", "advanced_soul_seal", "soul_suppress_talisman", "soul_suppress_talisman_basic", "preview_soul_seal", "镇魂法印", "镇魂符 family 的进阶 / 分支", "镇魂法印扩展镇魂符的稳定与压制。", control: true);

            yield return Profile("peach_wood_basic", "桃木牌", "legacy_peach_wood", "peach_wood", "peach_wood_basic", "preview_taomu_sword", "桃木剑", "桃木牌 family 的进阶 / 分支", "基础防护、护盾与抗邪反馈。", required: true, shield: true, control: true);
            yield return Profile("preview_taomu_sword", "桃木剑", "advanced_taomu_sword", "peach_wood", "peach_wood_basic", "preview_taomu_sword", "桃木剑", "桃木牌 family 的进阶 / 分支", "桃木剑把桃木牌防护扩展为破邪输出。", damage: true, breakShield: true, shield: true, control: true);

            yield return Profile("spirit_stone_basic", "聚灵石", "legacy_spirit_stone", "spirit_stone", "spirit_stone_basic", string.Empty, string.Empty, "聚灵石 / 聚能石家族为正式供能源", "正式供能源，只负责灵力回流，不承担主要输出。", required: true, mana: true, formalProvider: true);

            yield return Profile("shield_talisman_basic", "护身符", "legacy_shield_talisman", "guardian_ward", "shield_talisman_basic", "preview_x2_wood_talisman", "护阵木牌", "护身符 / 护阵 family 的支援扩展", "基础护盾与防御反馈。", shield: true);
            yield return Profile("preview_x2_wood_talisman", "护阵木牌", "support_wood_talisman", "guardian_ward", "shield_talisman_basic", "preview_x2_wood_talisman", "护阵木牌", "护身符 / 护阵 family 的支援扩展", "护阵木牌扩展基础护阵。", shield: true);
            yield return Profile("preview_guard_wood", "守护木牌", "support_guard_wood", "guardian_ward", "shield_talisman_basic", "preview_guard_wood", "守护木牌", "护身符 / 护阵 family 的支援扩展", "守护木牌扩展基础防御。", shield: true);

            yield return Profile("qi_pill_basic", "丹药", "legacy_qi_pill", "pill", "qi_pill_basic", string.Empty, string.Empty, "旧版恢复类基础件", "回气调息与轻度恢复反馈。", cleanse: true);
            yield return Profile("water_talisman_basic", "水符", "legacy_water_talisman", "water_talisman", "water_talisman_basic", string.Empty, string.Empty, "旧版水系净化参考件", "水气流转与净化恢复反馈。", cleanse: true);
            yield return Profile("sword_pill_basic", "剑丸", "legacy_sword_pill", "sword_pill", "sword_pill_basic", string.Empty, string.Empty, "旧版攻击参考件", "飞剑攻击与轻度破势反馈。", damage: true, breakShield: true);
            yield return Profile("seal_basic", "法印", "legacy_seal", "seal", "seal_basic", string.Empty, string.Empty, "旧版法印基础件，和镇魂符 family 分开保留", "法印维持阵面联动，不改为供能 provider。", control: true);

            yield return Profile("preview_energy_incense", "醒符香", "support_energy_incense", "awakening_incense", string.Empty, string.Empty, string.Empty, "辅助 / 节奏件，不是正式供能源", "醒符香只提供节奏辅助反馈，不作为正式供能源。", control: true, auxiliaryNonProvider: true);
            yield return Profile("preview_stone_core", "炉芯石", "support_stone_core", "furnace_core", string.Empty, string.Empty, string.Empty, "辅助 / 核心机制件，不是正式供能源", "炉芯石只做核心机制反馈，不作为正式供能源。", shield: true, control: true, auxiliaryNonProvider: true);
        }

        private static LegacyItemBehaviorProfile Profile(
            string itemId,
            string chinese,
            string stableKey,
            string family,
            string baseItemId,
            string advancedItemId,
            string advancedName,
            string relationship,
            string summary,
            bool required = false,
            bool damage = false,
            bool breakShield = false,
            bool cleanse = false,
            bool control = false,
            bool shield = false,
            bool mana = false,
            bool formalProvider = false,
            bool auxiliaryNonProvider = false)
        {
            return new LegacyItemBehaviorProfile
            {
                itemId = itemId ?? string.Empty,
                chineseName = chinese ?? string.Empty,
                englishStableKey = stableKey ?? NormalizeStableKey(itemId),
                itemFamily = family ?? string.Empty,
                baseItemId = baseItemId ?? string.Empty,
                advancedItemId = advancedItemId ?? string.Empty,
                advancedItemChineseName = advancedName ?? string.Empty,
                advancedRelationshipChinese = relationship ?? string.Empty,
                behaviorSummaryChinese = summary ?? string.Empty,
                requiredCoreItem = required,
                dealsEnemyDamage = damage,
                breaksShield = breakShield,
                cleansesNegative = cleanse,
                controlsEnemy = control,
                grantsShield = shield,
                restoresMana = mana,
                formalEnergyProvider = formalProvider,
                auxiliaryNonProvider = auxiliaryNonProvider,
                devOnly = true,
                isEnabled = false
            };
        }

        private static string BuildPlayerFeedback(
            LegacyItemBehaviorProfile profile,
            EnergyState state,
            int efficiency)
        {
            string name = profile?.chineseName ?? "道具";
            switch (state)
            {
                case EnergyState.Powered:
                    return $"聚能石连通，{name}触发稳定。";
                case EnergyState.WeakPulse:
                    return $"符阵微亮，{name}只亮起一部分，效果较弱。";
                case EnergyState.Suppressed:
                    return $"供能被压住，{name}暂不触发。";
                case EnergyState.None:
                default:
                    return $"供能断开，{name}沉寂。";
            }
        }

        private static string BuildCombatLog(
            LegacyItemBehaviorProfile profile,
            EnergyState state,
            int efficiency)
        {
            string name = profile?.chineseName ?? "道具";
            string summary = profile?.behaviorSummaryChinese ?? "基础行为反馈";
            switch (state)
            {
                case EnergyState.Powered:
                    return profile?.formalEnergyProvider == true || profile?.auxiliaryNonProvider == true
                        ? $"【{name}】{summary}稳定生效。"
                        : $"【{name}】{summary}稳定生效，阵势回响接上。";
                case EnergyState.WeakPulse:
                    return $"【{name}】{summary}弱脉生效，效果较弱。";
                case EnergyState.Suppressed:
                    return $"【{name}】供能被压住，本次不生效。";
                case EnergyState.None:
                default:
                    return $"【{name}】供能断开，本次沉寂。";
            }
        }

        private static string BuildSampleResult(LegacyItemBehaviorResult result)
        {
            if (result == null || !result.triggers)
            {
                return "no_trigger";
            }

            List<string> parts = new();
            if (result.enemyHpDamage > 0)
            {
                parts.Add($"enemyHpDamage={result.enemyHpDamage}");
            }

            if (result.enemyShieldPressure > 0)
            {
                parts.Add($"enemyShieldPressure={result.enemyShieldPressure}");
            }

            if (result.playerShieldGain > 0)
            {
                parts.Add($"playerShieldGain={result.playerShieldGain}");
            }

            if (result.cleanseValue > 0)
            {
                parts.Add($"cleanse={result.cleanseValue}");
            }

            if (result.controlValue > 0)
            {
                parts.Add($"control={result.controlValue}");
            }

            if (result.manaGain > 0)
            {
                parts.Add($"manaGain={result.manaGain}");
            }

            if (parts.Count == 0)
            {
                parts.Add("feedback_only");
            }

            return string.Join(";", parts);
        }

        private static bool IsSpiritStoneIdentity(string itemId, string family, string baseItemId)
        {
            return string.Equals(itemId, "spirit_stone_basic", StringComparison.Ordinal)
                || string.Equals(baseItemId, "spirit_stone_basic", StringComparison.Ordinal)
                || string.Equals(family, "spirit_stone", StringComparison.Ordinal)
                || string.Equals(family, "energy_stone", StringComparison.Ordinal);
        }

        private static bool IsAuxiliaryNonProvider(string itemId, string family)
        {
            string joined = $"{itemId ?? string.Empty} {family ?? string.Empty}";
            return joined.IndexOf("energy_incense", StringComparison.OrdinalIgnoreCase) >= 0
                || joined.IndexOf("awakening_incense", StringComparison.OrdinalIgnoreCase) >= 0
                || joined.IndexOf("stone_core", StringComparison.OrdinalIgnoreCase) >= 0
                || joined.IndexOf("furnace_core", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string NormalizeStableKey(string itemId)
        {
            string safe = string.IsNullOrWhiteSpace(itemId) ? "sandbox_item" : itemId.Trim();
            return safe.Replace("__", "_").ToLowerInvariant();
        }
    }
}
