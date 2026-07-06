using System;
using System.Collections.Generic;
using System.Linq;

namespace TalismanBag.BuildSandbox
{
    [Serializable]
    public sealed class BattleSandboxItemEffectRuntimeProfile
    {
        public string itemEffectKey = string.Empty;
        public string effectFamilyKey = string.Empty;
        public string displayNameChinese = string.Empty;
        public string effectFamilyChinese = string.Empty;
        public string effectRoleChinese = string.Empty;
        public string triggerVerbChinese = string.Empty;
        public string triggerFloatingTextChinese = string.Empty;
        public string triggerStateLineChinese = string.Empty;
        public string triggerCastLineChinese = string.Empty;
        public string damageStateLineChinese = string.Empty;
        public string damageCastLineChinese = string.Empty;
        public string damageFloatingPrefixChinese = string.Empty;
        public string shieldStateLineChinese = string.Empty;
        public string shieldFloatingPrefixChinese = string.Empty;
        public string healStateLineChinese = string.Empty;
        public string healFloatingPrefixChinese = string.Empty;
        public string passiveLogChinese = string.Empty;
        public string passiveFloatingTextChinese = string.Empty;
        public bool dealsEnemyDamage;
        public bool grantsShield;
        public bool restoresMana;
        public bool cleanses;
        public bool controlsBoss;
        public bool breaksShield;
        public bool supportOnly;
        public bool devOnly = true;
        public bool isEnabled;

        public string SourceDataPath =>
            $"{BattleSandboxItemEffectRuntimePreviewCatalog.SourceDataPath}.{itemEffectKey}";
    }

    public static class BattleSandboxItemEffectRuntimePreviewCatalog
    {
        public const string PackageName = "V0.4-BattleSandboxItemEffectRuntimePreview01";
        public const string SourceDataPath = "BattleSandboxItemEffectRuntimePreviewCatalog";
        public const string DeveloperDataPanelFieldKey = "battleSandboxItemEffectRuntimePreview";

        private static readonly Dictionary<string, BattleSandboxItemEffectRuntimeProfile> ProfilesByItemId =
            BuildProfiles().ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        public static IReadOnlyList<BattleSandboxItemEffectRuntimeProfile> AllProfiles =>
            ProfilesByItemId.Values.ToArray();

        public static IReadOnlyList<string> MappedItemIds => ProfilesByItemId.Keys.ToArray();

        public static BattleSandboxItemEffectRuntimeProfile Resolve(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat = null)
        {
            return Resolve(item?.itemId, item?.itemFamily, stat?.statTag);
        }

        public static BattleSandboxItemEffectRuntimeProfile Resolve(
            string itemId,
            string itemFamily = "",
            string statTag = "")
        {
            string safeItemId = itemId ?? string.Empty;
            if (ProfilesByItemId.TryGetValue(safeItemId, out BattleSandboxItemEffectRuntimeProfile profile))
            {
                return profile;
            }

            string joined = $"{safeItemId} {itemFamily ?? string.Empty} {statTag ?? string.Empty}";
            if (ContainsAny(joined, "fire"))
            {
                return ProfilesByItemId["fire_talisman_basic"];
            }

            if (ContainsAny(joined, "thunder", "shieldbreak", "break"))
            {
                return ProfilesByItemId["thunder_talisman_basic"];
            }

            if (ContainsAny(joined, "cleanse", "purify", "water", "heal"))
            {
                return ProfilesByItemId["purify_talisman_basic"];
            }

            if (ContainsAny(joined, "spirit_stone", "energy_stone", "spirit"))
            {
                return ProfilesByItemId["spirit_stone_basic"];
            }

            if (ContainsAny(joined, "energy", "stone_core", "incense", "core"))
            {
                return ProfilesByItemId["preview_energy_incense"];
            }

            if (ContainsAny(joined, "guard", "shield", "ward", "wood_talisman"))
            {
                return ProfilesByItemId["shield_talisman_basic"];
            }

            if (ContainsAny(joined, "soul", "seal", "bell", "control"))
            {
                return ProfilesByItemId["soul_suppress_talisman_basic"];
            }

            if (ContainsAny(joined, "peach", "taomu"))
            {
                return ProfilesByItemId["peach_wood_basic"];
            }

            return Create(
                "sandbox_support",
                "support",
                "道具",
                "辅助",
                "辅助",
                "维持阵面",
                "辅助生效",
                "道具：辅助生效",
                "首领继续蓄力",
                "敌人：状态观察",
                "首领状态变化",
                "辅助",
                "玩家：阵面稳定",
                "护阵",
                "玩家：状态回稳",
                "恢复",
                "【辅助】道具维持当前阵面",
                "辅助");
        }

        public static bool IsPassiveRuntimeEffect(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Resolve(item, stat);
            BuildSandboxItemStat safeStat = stat ?? BuildSandboxItemStatCatalog.Resolve(item?.itemId);
            return safeStat.manaCostPerCast <= 0
                || profile.restoresMana
                || (profile.supportOnly && !profile.dealsEnemyDamage && safeStat.attack <= 0);
        }

        public static bool CanDealEnemyDamage(
            BuildSandboxPlacedItemSnapshot item,
            BuildSandboxItemStat stat)
        {
            return Resolve(item, stat).dealsEnemyDamage;
        }

        public static string DisplayItem(BuildSandboxPlacedItemSnapshot item, BuildSandboxItemStat stat = null)
        {
            string display = Resolve(item, stat).displayNameChinese;
            return string.IsNullOrWhiteSpace(display) ? "未知道具" : display;
        }

        public static string DisplayShortItem(BuildSandboxPlacedItemSnapshot item, BuildSandboxItemStat stat = null)
        {
            string display = DisplayItem(item, stat);
            return string.IsNullOrWhiteSpace(display) ? "道具" : display;
        }

        private static IEnumerable<KeyValuePair<string, BattleSandboxItemEffectRuntimeProfile>> BuildProfiles()
        {
            yield return Pair("fire_talisman_basic", Fire("fire_basic", "火符", "引燃符火"));
            yield return Pair("preview_fire_talisman", Fire("fire_burst", "炽火符", "卷起炽火"));

            yield return Pair("thunder_talisman_basic", Thunder("thunder_basic", "雷符", "落下雷击"));
            yield return Pair("chain_thunder_talisman_basic", Thunder("thunder_chain", "连锁雷符", "牵起连锁雷击"));
            yield return Pair("preview_thunder_sword", Thunder("thunder_sword", "雷引剑符", "牵雷破盾"));

            yield return Pair("sword_pill_basic", Attack("sword_pill", "sword", "剑丸", "剑击", "攻击", "飞剑出鞘", "剑丸飞击", "敌人：剑气命中", "首领护势被剑气切开", "剑击"));
            yield return Pair("exorcism_bell_basic", Attack("exorcism_bell", "exorcism", "驱邪铃", "驱邪", "驱邪", "震铃驱邪", "驱邪震响", "敌人：邪气被震散", "首领护势短暂松动", "驱邪"));
            yield return Pair("preview_taomu_sword", Attack("taomu_sword", "peach", "桃木剑", "桃木", "破邪", "挥出桃木斩", "桃木斩", "敌人：邪气被桃木压住", "首领护势被破邪气息压低", "桃木斩"));

            yield return Pair("shield_talisman_basic", Guard("guard_basic", "护身符", "撑起护盾", "护盾展开"));
            yield return Pair("preview_x2_wood_talisman", Guard("guard_wood_plate", "护阵木牌", "稳住阵脚", "护阵"));
            yield return Pair("preview_guard_wood", Guard("guard_wood", "守护木牌", "撑开守护木纹", "守护"));

            yield return Pair("qi_pill_basic", Cleanse("qi_pill", "丹药", "回气调息", "回气"));
            yield return Pair("water_talisman_basic", Cleanse("water_talisman", "水符", "水气流转", "水气"));
            yield return Pair("purify_talisman_basic", Cleanse("purify_basic", "净化符", "净化负面气息", "净化"));
            yield return Pair("preview_cleanse_corner", Cleanse("cleanse_corner", "净化折符", "折光净化", "净化"));

            yield return Pair("spirit_stone_basic", Energy("spirit_stone", "聚灵石", "聚起灵气", "灵气"));
            yield return Pair("preview_energy_incense", NonEnergySupport("energy_incense", "醒符香", "醒符牵引阵脉", "阵脉牵引"));
            yield return Pair("preview_stone_core", NonEnergySupport("stone_core", "炉芯石", "炉芯稳定阵面", "炉芯稳定"));

            yield return Pair("soul_suppress_talisman_basic", Control("soul_suppress", "镇魂符", "镇魂压制", "镇魂"));
            yield return Pair("seal_basic", Control("seal_basic", "法印", "维持法印", "法印"));
            yield return Pair("preview_soul_seal", Control("soul_seal", "镇魂法印", "法印镇魂", "镇魂"));
            yield return Pair("preview_old_bell", Control("old_bell", "镇邪铃", "镇邪压制", "镇邪"));
            yield return Pair("peach_wood_basic", PeachSupport("peach_wood_basic", "桃木牌", "桃木破邪", "破邪"));
        }

        private static KeyValuePair<string, BattleSandboxItemEffectRuntimeProfile> Pair(
            string itemId,
            BattleSandboxItemEffectRuntimeProfile profile)
        {
            return new KeyValuePair<string, BattleSandboxItemEffectRuntimeProfile>(itemId, profile);
        }

        private static BattleSandboxItemEffectRuntimeProfile Fire(
            string key,
            string display,
            string verb)
        {
            return Attack(key, "fire", display, "火焰", "攻击", verb, "火焰爆开", "敌人：火焰灼伤", "首领护势被火焰逼退", "火焰");
        }

        private static BattleSandboxItemEffectRuntimeProfile Thunder(
            string key,
            string display,
            string verb)
        {
            BattleSandboxItemEffectRuntimeProfile profile =
                Attack(key, "thunder", display, "雷击", "破盾", verb, "雷光劈落", "敌人：雷击破盾", "首领护势被雷引打断", "雷击");
            profile.breaksShield = true;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile Attack(
            string key,
            string familyKey,
            string display,
            string family,
            string role,
            string verb,
            string floating,
            string damageState,
            string damageCast,
            string damagePrefix)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Create(
                key,
                familyKey,
                display,
                family,
                role,
                verb,
                floating,
                $"道具：{family}发动",
                "首领蓄力被压住",
                damageState,
                damageCast,
                damagePrefix,
                "玩家：阵面稳定",
                "护阵",
                "玩家：状态回稳",
                "恢复",
                $"【{family}】{display}{verb}",
                floating);
            profile.dealsEnemyDamage = true;
            profile.supportOnly = false;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile Guard(
            string key,
            string display,
            string verb,
            string floating)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Create(
                key,
                "guard",
                display,
                "护阵",
                "防御",
                verb,
                floating,
                "道具：护阵生效",
                "首领攻势被顶住",
                "敌人：护势观察",
                "首领护势重组",
                "护阵",
                "玩家：护盾更新",
                "护盾",
                "玩家：状态回稳",
                "恢复",
                $"【护阵】{display}{verb}",
                floating);
            profile.grantsShield = true;
            profile.supportOnly = true;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile Cleanse(
            string key,
            string display,
            string verb,
            string floating)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Create(
                key,
                "cleanse",
                display,
                "净化",
                "净化",
                verb,
                floating,
                "道具：净化流转",
                "首领蓄力未断",
                "敌人：净化压制",
                "首领邪气被洗开",
                "净化",
                "玩家：护阵微亮",
                "护阵",
                "玩家：气血回稳",
                "净化",
                $"【净化】{display}{verb}",
                floating);
            profile.cleanses = true;
            profile.supportOnly = true;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile Energy(
            string key,
            string display,
            string verb,
            string floating)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Create(
                key,
                "energy",
                display,
                "供能",
                "供能",
                verb,
                floating,
                "道具：灵力供给",
                "首领蓄力中",
                "敌人：承压观察",
                "首领护势被灵流压住",
                "灵力",
                "玩家：灵力回流",
                "护阵",
                "玩家：灵气回流",
                "灵气",
                $"【供能】{display}{verb}",
                floating);
            profile.restoresMana = true;
            profile.supportOnly = true;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile NonEnergySupport(
            string key,
            string display,
            string verb,
            string floating)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Create(
                key,
                "support",
                display,
                "辅助",
                "辅助",
                verb,
                floating,
                "道具：辅助阵面",
                "首领继续蓄力",
                "敌人：状态观察",
                "首领状态变化",
                "辅助",
                "玩家：阵面稳定",
                "护阵",
                "玩家：状态回稳",
                "辅助",
                $"【辅助】{display}{verb}",
                floating);
            profile.supportOnly = true;
            profile.restoresMana = false;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile Control(
            string key,
            string display,
            string verb,
            string floating)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Create(
                key,
                "control",
                display,
                "镇魂",
                "控制",
                verb,
                floating,
                "道具：控制压制",
                "首领施法被压慢",
                "敌人：行动受压",
                "首领施法条被镇住",
                "压制",
                "玩家：护阵稳定",
                "护阵",
                "玩家：状态回稳",
                "镇魂",
                $"【镇魂】{display}{verb}",
                floating);
            profile.controlsBoss = true;
            profile.supportOnly = true;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile PeachSupport(
            string key,
            string display,
            string verb,
            string floating)
        {
            BattleSandboxItemEffectRuntimeProfile profile = Create(
                key,
                "peach",
                display,
                "桃木",
                "破邪",
                verb,
                floating,
                "道具：桃木破邪",
                "首领邪气被压住",
                "敌人：邪气被压低",
                "首领护势短暂收紧",
                "破邪",
                "玩家：护阵稳定",
                "护阵",
                "玩家：状态回稳",
                "破邪",
                $"【桃木】{display}{verb}",
                floating);
            profile.controlsBoss = true;
            profile.supportOnly = true;
            return profile;
        }

        private static BattleSandboxItemEffectRuntimeProfile Create(
            string key,
            string familyKey,
            string display,
            string family,
            string role,
            string verb,
            string triggerFloating,
            string triggerState,
            string triggerCast,
            string damageState,
            string damageCast,
            string damagePrefix,
            string shieldState,
            string shieldPrefix,
            string healState,
            string healPrefix,
            string passiveLog,
            string passiveFloating)
        {
            return new BattleSandboxItemEffectRuntimeProfile
            {
                itemEffectKey = key ?? string.Empty,
                effectFamilyKey = familyKey ?? string.Empty,
                displayNameChinese = display ?? string.Empty,
                effectFamilyChinese = family ?? string.Empty,
                effectRoleChinese = role ?? string.Empty,
                triggerVerbChinese = verb ?? string.Empty,
                triggerFloatingTextChinese = triggerFloating ?? string.Empty,
                triggerStateLineChinese = triggerState ?? string.Empty,
                triggerCastLineChinese = triggerCast ?? string.Empty,
                damageStateLineChinese = damageState ?? string.Empty,
                damageCastLineChinese = damageCast ?? string.Empty,
                damageFloatingPrefixChinese = damagePrefix ?? string.Empty,
                shieldStateLineChinese = shieldState ?? string.Empty,
                shieldFloatingPrefixChinese = shieldPrefix ?? string.Empty,
                healStateLineChinese = healState ?? string.Empty,
                healFloatingPrefixChinese = healPrefix ?? string.Empty,
                passiveLogChinese = passiveLog ?? string.Empty,
                passiveFloatingTextChinese = passiveFloating ?? string.Empty,
                devOnly = true,
                isEnabled = false
            };
        }

        private static bool ContainsAny(string value, params string[] tokens)
        {
            string safeValue = value ?? string.Empty;
            foreach (string token in tokens ?? Array.Empty<string>())
            {
                if (!string.IsNullOrWhiteSpace(token)
                    && safeValue.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Serializable]
    public sealed class BattleSandboxItemEffectRuntimePreviewSnapshot
    {
        public string packageName = BattleSandboxItemEffectRuntimePreviewCatalog.PackageName;
        public bool devOnly = true;
        public bool isEnabled;
        public bool reusesRuntimeLoopFeedback = true;
        public bool reusesExistingHudText = true;
        public bool createsNewUiFrame;
        public bool writesFormalFlow;
        public bool writesFormalSaveData;
        public bool grantsFormalReward;
        public bool advancesChapter;
        public bool touchesFormalSceneUiLayout;
        public int rosterItemCount;
        public int profileCount;
        public int mappedRosterItemCount;
        public int runtimeRowCount;
        public int itemEffectRuntimeRowCount;
        public int distinctEffectFamilyCount;
        public int attackProfileCount;
        public int supportProfileCount;
        public int energyProfileCount;
        public int guardProfileCount;
        public int cleanseProfileCount;
        public int controlProfileCount;
        public int peachProfileCount;
        public int supportDamageLeakCount;
        public int playerSideAnswerLeakCount;
        public int formalLeakCount;
        public int uiLayoutWriteCount;
        public List<BattleSandboxItemEffectRuntimeProfileRow> profileRows = new();
        public List<BattleSandboxItemEffectRuntimeSampleRow> sampleRows = new();

        public bool Passed =>
            devOnly
            && !isEnabled
            && reusesRuntimeLoopFeedback
            && reusesExistingHudText
            && !createsNewUiFrame
            && rosterItemCount == BattleSandboxPlayableFullRosterRegression.ExpectedRosterItemCount
            && profileCount >= rosterItemCount
            && mappedRosterItemCount == rosterItemCount
            && itemEffectRuntimeRowCount >= rosterItemCount
            && distinctEffectFamilyCount >= 8
            && attackProfileCount >= BattleSandboxPlayableFullRosterRegression.ExpectedAttackDamageItemIds.Length
            && supportProfileCount >= BattleSandboxPlayableFullRosterRegression.ExpectedNoDamageSupportItemIds.Length
            && energyProfileCount >= 1
            && guardProfileCount >= 3
            && cleanseProfileCount >= 4
            && controlProfileCount >= 4
            && peachProfileCount >= 2
            && supportDamageLeakCount == 0
            && playerSideAnswerLeakCount == 0
            && formalLeakCount == 0
            && uiLayoutWriteCount == 0
            && !touchesFormalSceneUiLayout;
    }

    [Serializable]
    public sealed class BattleSandboxItemEffectRuntimeProfileRow
    {
        public string itemId = string.Empty;
        public string displayNameChinese = string.Empty;
        public string itemEffectKey = string.Empty;
        public string effectFamilyKey = string.Empty;
        public string effectFamilyChinese = string.Empty;
        public string effectRoleChinese = string.Empty;
        public string statProfileId = string.Empty;
        public string statTag = string.Empty;
        public bool dealsEnemyDamage;
        public bool supportOnly;
        public bool restoresMana;
        public bool grantsShield;
        public bool cleanses;
        public bool controlsBoss;
        public bool breaksShield;
        public bool devOnly = true;
        public bool isEnabled;
        public string sourceDataPath = string.Empty;
    }

    [Serializable]
    public sealed class BattleSandboxItemEffectRuntimeSampleRow
    {
        public string itemId = string.Empty;
        public string itemEffectKey = string.Empty;
        public string effectFamilyChinese = string.Empty;
        public string effectRoleChinese = string.Empty;
        public int runtimeRows;
        public int effectRows;
        public int enemyHpDamageTotal;
        public bool expectedAttackDamage;
        public bool expectedSupportNoDamage;
        public int playerSideAnswerLeakCount;
        public string sampleFloatingChinese = string.Empty;
        public string sampleLogChinese = string.Empty;
        public bool passed;
    }
}
