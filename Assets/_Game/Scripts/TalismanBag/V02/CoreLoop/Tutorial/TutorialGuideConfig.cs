using System;
using System.Collections.Generic;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Rewards;
using UnityEngine;

namespace TalismanBag.V02.CoreLoop.Tutorial
{
    public enum TutorialGuideTrigger
    {
        PreBattle,
        RoundWin,
        BossClear
    }

    [Serializable]
    public sealed class TutorialGuideRow
    {
        public bool enabled = true;
        public string levelId;
        public TutorialGuideTrigger trigger;
        public string onceFlag;
        public bool consumeOnce = true;
        public bool markChapterOneBossClear;
        public string highestClearedLevelIdOnComplete;

        [Header("Panel")]
        public string panelTitle;
        public string panelSubject;
        [TextArea] public string panelDescription;

        [Header("Reward")]
        public RewardConfig rewardConfig;
        public bool blockFlowWhenRuntimeInventorySyncFails = true;

        public bool HasReward => rewardConfig?.rewards != null && rewardConfig.rewards.Count > 0;

        public string GetSafeLevelId()
        {
            return string.IsNullOrWhiteSpace(levelId) ? string.Empty : levelId.Trim();
        }

        public string GetSafeOnceFlag()
        {
            if (!string.IsNullOrWhiteSpace(onceFlag))
            {
                return onceFlag.Trim();
            }

            string safeLevelId = GetSafeLevelId();
            return string.IsNullOrEmpty(safeLevelId) ? string.Empty : $"{trigger}_{safeLevelId}";
        }
    }

    [CreateAssetMenu(menuName = "TalismanBag/V02/CoreLoop/Tutorial Guide Config", fileName = "TutorialGuideConfig")]
    public sealed class TutorialGuideConfig : ScriptableObject
    {
        public string configId = "fix03_chapter_one_guide";
        public List<TutorialGuideRow> rows = new();

        public TutorialGuideRow FindRow(string levelId, TutorialGuideTrigger trigger)
        {
            string safeLevelId = NormalizeLevelId(levelId);
            if (string.IsNullOrEmpty(safeLevelId) || rows == null)
            {
                return null;
            }

            foreach (TutorialGuideRow row in rows)
            {
                if (row == null || !row.enabled || row.trigger != trigger)
                {
                    continue;
                }

                if (string.Equals(row.GetSafeLevelId(), safeLevelId, StringComparison.Ordinal))
                {
                    return row;
                }
            }

            return null;
        }

        public static TutorialGuideConfig CreateFix03RuntimeDefaults(RewardConfig chapterOneBossRewardOverride = null)
        {
            TutorialGuideConfig config = CreateInstance<TutorialGuideConfig>();
            config.hideFlags = HideFlags.DontSave;
            config.configId = "fix03_chapter_one_guide_runtime";
            config.rows = new List<TutorialGuideRow>
            {
                CreatePreBattleRow(
                    "1-1",
                    "tutorial_prebattle_1_1_energy_stone",
                    "聚能石说明",
                    "聚能石 / 聚灵石",
                    "聚能石会为附近符箓提供灵力。\n只有获得灵力的符箓才能发动。\n请让火符处在聚能石供能范围内。"),
                CreateFixedRewardRow(
                    "1-2",
                    "fixed_reward_1_2_thunder_talisman",
                    "thunder_talisman_basic",
                    "雷符",
                    "雷符擅长击破护盾。\n下一关敌人带有护盾，请将雷符接入供能。"),
                CreateFixedRewardRow(
                    "1-4",
                    "fixed_reward_1_4_shield_talisman",
                    "shield_talisman_basic",
                    "护身符",
                    "护身符可以提高生存能力。\n下一关敌人攻击更强，请将护身符接入供能。"),
                CreateFixedRewardRow(
                    "1-5",
                    "fixed_reward_1_5_purify_talisman",
                    "purify_talisman_basic",
                    "净化符",
                    "净化符可以清除毒、燃烧等负面状态。\n下一关敌人会施加负面状态，请准备净化符。"),
                CreateFixedRewardRow(
                    "1-6",
                    "fixed_reward_1_6_soul_suppress_talisman",
                    "soul_suppress_talisman_basic",
                    "镇魂符",
                    "镇魂符可以压制偷灵、封符等异常干扰。\n下一关敌人会干扰你的阵势，请准备镇魂符。"),
                CreateFixedRewardRow(
                    "1-8",
                    "fixed_reward_1_8_chain_thunder_talisman",
                    "chain_thunder_talisman_basic",
                    "连锁雷符",
                    "连锁雷符可以弹射攻击多个敌人。\n前方将进入综合试炼，请尝试用它处理多目标压力。"),
                CreateBossClearRow(chapterOneBossRewardOverride)
            };

            return config;
        }

        private static TutorialGuideRow CreatePreBattleRow(
            string levelId,
            string onceFlag,
            string title,
            string subject,
            string description)
        {
            return new TutorialGuideRow
            {
                levelId = levelId,
                trigger = TutorialGuideTrigger.PreBattle,
                onceFlag = onceFlag,
                panelTitle = title,
                panelSubject = subject,
                panelDescription = description,
                blockFlowWhenRuntimeInventorySyncFails = false
            };
        }

        private static TutorialGuideRow CreateFixedRewardRow(
            string levelId,
            string onceFlag,
            string itemId,
            string itemName,
            string description)
        {
            return new TutorialGuideRow
            {
                levelId = levelId,
                trigger = TutorialGuideTrigger.RoundWin,
                onceFlag = onceFlag,
                panelTitle = $"获得：{itemName}",
                panelSubject = $"{itemName} x1",
                panelDescription = description,
                rewardConfig = CreateSingleItemRewardConfig(levelId, itemId, itemName, description),
                blockFlowWhenRuntimeInventorySyncFails = true
            };
        }

        private static TutorialGuideRow CreateBossClearRow(RewardConfig rewardOverride)
        {
            return new TutorialGuideRow
            {
                levelId = "1-10",
                trigger = TutorialGuideTrigger.BossClear,
                onceFlag = "chapter_1_10_clear",
                markChapterOneBossClear = true,
                highestClearedLevelIdOnComplete = "1-10",
                panelTitle = "1-10 Boss 已击破",
                panelSubject = "章节奖励",
                panelDescription = "领取章节结算奖励后完成 1-10 主线试炼。",
                rewardConfig = rewardOverride != null ? rewardOverride : CreateChapterOneBossRewardConfig(),
                blockFlowWhenRuntimeInventorySyncFails = false
            };
        }

        private static RewardConfig CreateSingleItemRewardConfig(
            string levelId,
            string itemId,
            string itemName,
            string description)
        {
            RewardConfig rewardConfig = CreateInstance<RewardConfig>();
            rewardConfig.hideFlags = HideFlags.DontSave;
            rewardConfig.rewardId = $"fixed_tutorial_{NormalizeLevelId(levelId).Replace("-", "_")}";
            rewardConfig.displayName = $"{levelId} 固定教学奖励";
            rewardConfig.description = description;
            rewardConfig.rewards = new List<RewardEntry>
            {
                new()
                {
                    rewardType = RewardType.Item,
                    itemId = itemId,
                    amount = 1,
                    displayName = itemName
                }
            };
            return rewardConfig;
        }

        private static RewardConfig CreateChapterOneBossRewardConfig()
        {
            RewardConfig rewardConfig = CreateInstance<RewardConfig>();
            rewardConfig.hideFlags = HideFlags.DontSave;
            rewardConfig.rewardId = "chapter_1_10_clear";
            rewardConfig.displayName = "1-10 章节结算";
            rewardConfig.description = "1-10 Boss 首通固定奖励";
            rewardConfig.rewards = new List<RewardEntry>
            {
                CreateItemReward("sword_pill_basic", "剑丸", 1),
                CreateResourceReward(ResourceType.SpiritStone, "灵石", 120),
                CreateResourceReward(ResourceType.TalismanPaper, "符纸", 60),
                CreateResourceReward(ResourceType.Cinnabar, "朱砂", 10),
                CreateResourceReward(ResourceType.BasicTalismanEmbryo, "初阶符胚", 1),
                CreateResourceReward(ResourceType.Cultivation, "修为", 20)
            };
            return rewardConfig;
        }

        private static RewardEntry CreateItemReward(string itemId, string displayName, int amount)
        {
            return new RewardEntry
            {
                rewardType = RewardType.Item,
                itemId = itemId,
                amount = amount,
                displayName = displayName
            };
        }

        private static RewardEntry CreateResourceReward(ResourceType resourceType, string displayName, int amount)
        {
            return new RewardEntry
            {
                rewardType = RewardType.Resource,
                resourceType = resourceType,
                amount = amount,
                displayName = displayName
            };
        }

        private static string NormalizeLevelId(string levelId)
        {
            return string.IsNullOrWhiteSpace(levelId) ? string.Empty : levelId.Trim();
        }
    }
}
