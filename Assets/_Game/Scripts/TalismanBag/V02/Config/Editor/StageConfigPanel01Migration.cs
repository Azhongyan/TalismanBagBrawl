#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Enemies;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.CoreLoop.Tutorial;
using TalismanBag.V02.EnemySkills;
using TalismanBag.V02.Run;
using TalismanBag.V02.Tags;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class StageConfigPanel01Migration
    {
        private const string RunConfigPath =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset";
        private const string StageEnemyRoot =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/StageConfigPanel01";
        private const string StageSkillRoot =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/Enemies/Skills/StageConfigPanel01";
        private const string EnemyGroupRoot =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/EnemyGroups";
        private const string RewardRoot =
            "Assets/_Game/Resources/CoreLoop/Rewards";

        public static void ExecuteBatch()
        {
            try
            {
                RunMigration();
                Debug.Log("[StageConfigPanel01] MIGRATION_SUCCESS");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                throw;
            }
        }

        public static void VerifyBatch()
        {
            V02RunConfig runConfig = AssetDatabase.LoadAssetAtPath<V02RunConfig>(RunConfigPath);
            Require(runConfig != null, "Missing verified V02 run config.");
            Require(runConfig.rounds != null && runConfig.rounds.Count == 10, "Chapter 1 must contain 10 stages.");
            Require(runConfig.chapterTwoRounds != null && runConfig.chapterTwoRounds.Count == 10, "Chapter 2 must contain 10 stages.");

            List<V02RoundConfig> allStages = new();
            allStages.AddRange(runConfig.rounds);
            allStages.AddRange(runConfig.chapterTwoRounds);
            Require(allStages.All(stage => stage != null && stage.stageConfigVersion > 0), "Every stage must use StageConfigPanel01 fields.");
            Require(allStages.Select(stage => stage.levelId).Distinct(StringComparer.Ordinal).Count() == 20, "Stage IDs must be unique.");
            Require(allStages.All(stage => stage.enemyGroup != null), "Every stage must reference an enemy group.");
            Require(allStages.All(stage => stage.enemyGroup.BuildPrimaryRuntimeEnemy() != null), "Every enemy group must build a runtime enemy.");

            V02RoundConfig chapterTwoNine = runConfig.chapterTwoRounds[8];
            V02RoundConfig chapterTwoBoss = runConfig.chapterTwoRounds[9];
            Require(chapterTwoNine.levelId == "2-9" && chapterTwoNine.stopBeforeBoss, "2-9 must stop before the boss.");
            Require(chapterTwoNine.nextStageId == "2-10", "2-9 must point to 2-10.");
            Require(chapterTwoBoss.levelId == "2-10" && chapterTwoBoss.isBossRound, "2-10 must remain a boss stage.");
            Require(!chapterTwoBoss.autoAdvance, "2-10 boss must not auto-start.");

            Require(RewardConfig.LoadById("chapter_1_10_clear") != null, "Missing 1-10 chapter reward config.");
            Require(RewardConfig.LoadById("boss_2_10_clear") != null, "Missing 2-10 boss reward config.");
            Require(UnityEngine.Resources.Load<TutorialGuideConfig>("CoreLoop/TutorialGuideConfig_Fix03") != null, "Missing tutorial guide resource config.");

            IReadOnlyList<DataCatalogValidationResult> validation = DataCatalogValidator.Validate(DataCatalog.Collect());
            Require(validation.All(result => result.Level != DataCatalogValidationLevel.Error), "DataCatalog contains Error results.");
            Debug.Log($"[StageConfigPanel01] SMOKE_SUCCESS stages={allStages.Count}, validation={validation.Count}");
        }

        private static void RunMigration()
        {
            EnsureFolder(StageEnemyRoot);
            EnsureFolder(StageSkillRoot);
            EnsureFolder(EnemyGroupRoot);
            EnsureFolder(RewardRoot);

            MarkExistingCatalogSources();
            CreateBronzeSealItem();
            Dictionary<string, RewardConfig> rewards = CreateRewardConfigs();
            CreateTutorialGuideConfig(rewards);
            MigrateStageConfig(rewards);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void MarkExistingCatalogSources()
        {
            List<TalismanItemDefinition> items = LoadAssets<TalismanItemDefinition>(
                "Assets/_Game/ScriptableObjects/TalismanBag");
            HashSet<string> v02ItemIds = new(
                items.Where(item => DataCatalog.GetPath(item).Contains("/V02/Items/"))
                    .Select(item => item.itemId),
                StringComparer.Ordinal);

            foreach (TalismanItemDefinition item in items)
            {
                string path = DataCatalog.GetPath(item);
                bool isV02 = path.Contains("/V02/Items/");
                item.sourceType = isV02 ? CatalogSourceType.Verified : CatalogSourceType.Legacy;
                item.isDebugOnly = false;
                item.isDeprecated = !isV02 && v02ItemIds.Contains(item.itemId);
                item.canUpgrade = IsUpgradeableCoreItem(item.itemId);
                EditorUtility.SetDirty(item);
            }

            foreach (EnemyDefinition enemy in LoadAssets<EnemyDefinition>(
                         "Assets/_Game/ScriptableObjects/TalismanBag"))
            {
                string path = DataCatalog.GetPath(enemy);
                bool isV02 = path.Contains("/V02/Enemies/");
                enemy.sourceType = isV02 ? CatalogSourceType.Verified : CatalogSourceType.Legacy;
                enemy.isDebugOnly = false;
                enemy.isDeprecated = path.EndsWith("/V02/v02_mountain_imp.asset", StringComparison.OrdinalIgnoreCase) ||
                                     enemy.enemyId.StartsWith("Deprecated_", StringComparison.OrdinalIgnoreCase) ||
                                     enemy.enemyId.IndexOf("placeholder", StringComparison.OrdinalIgnoreCase) >= 0;
                EditorUtility.SetDirty(enemy);
            }
        }

        private static bool IsUpgradeableCoreItem(string itemId)
        {
            return itemId is "fire_talisman_basic" or
                "thunder_talisman_basic" or
                "shield_talisman_basic" or
                "purify_talisman_basic" or
                "soul_suppress_talisman_basic";
        }

        private static void CreateBronzeSealItem()
        {
            const string path =
                "Assets/_Game/ScriptableObjects/TalismanBag/V02/Items/bronze_seal_basic.asset";
            TalismanItemDefinition item = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<TalismanItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.itemId = "bronze_seal_basic";
            item.displayName = "青铜法印";
            item.enabled = true;
            item.itemType = TalismanItemType.PassiveTool;
            item.width = 1;
            item.height = 1;
            item.baseCooldown = 1f;
            item.baseValue = 0;
            item.description = "2-10 Boss 首通库存道具；当前仅入库存档，后续系统复用此定义。";
            item.shortRoleDescription = "章节首通库存道具";
            item.canDrop = false;
            item.canUpgrade = false;
            item.unlockChapter = 2;
            item.sourceType = CatalogSourceType.Verified;
            item.isDebugOnly = false;
            item.isDeprecated = false;
            EditorUtility.SetDirty(item);
        }

        private static Dictionary<string, RewardConfig> CreateRewardConfigs()
        {
            Dictionary<string, RewardConfig> rewards = new(StringComparer.Ordinal)
            {
                ["fixed_tutorial_1_2"] = CreateItemReward(
                    "fixed_tutorial_1_2",
                    "1-2 固定教学奖励",
                    "fixed_reward_1_2_thunder_talisman",
                    "thunder_talisman_basic",
                    "雷符",
                    "雷符擅长击破护盾。下一关敌人带有护盾，请将雷符接入供能。"),
                ["fixed_tutorial_1_4"] = CreateItemReward(
                    "fixed_tutorial_1_4",
                    "1-4 固定教学奖励",
                    "fixed_reward_1_4_shield_talisman",
                    "shield_talisman_basic",
                    "护身符",
                    "护身符可以提高生存能力。下一关敌人攻击更强，请将护身符接入供能。"),
                ["fixed_tutorial_1_5"] = CreateItemReward(
                    "fixed_tutorial_1_5",
                    "1-5 固定教学奖励",
                    "fixed_reward_1_5_purify_talisman",
                    "purify_talisman_basic",
                    "净化符",
                    "净化符可以清除毒、燃烧等负面状态。下一关敌人会施加负面状态，请准备净化符。"),
                ["fixed_tutorial_1_6"] = CreateItemReward(
                    "fixed_tutorial_1_6",
                    "1-6 固定教学奖励",
                    "fixed_reward_1_6_soul_suppress_talisman",
                    "soul_suppress_talisman_basic",
                    "镇魂符",
                    "镇魂符可以压制偷灵、封符等异常干扰。下一关敌人会干扰你的阵势，请准备镇魂符。"),
                ["fixed_tutorial_1_9"] = CreateItemReward(
                    "fixed_tutorial_1_9",
                    "1-9 固定教学奖励",
                    "fixed_reward_1_9_chain_thunder_talisman",
                    "chain_thunder_talisman_basic",
                    "连锁雷符",
                    "连锁雷符可以弹射攻击多个敌人。前方将进入综合试炼，请尝试用它处理多目标压力。"),
                ["chapter_1_10_clear"] = CreateChapterOneBossReward(),
                ["boss_2_10_clear"] = CreateChapterTwoBossReward()
            };
            return rewards;
        }

        private static RewardConfig CreateItemReward(
            string rewardId,
            string displayName,
            string onceFlag,
            string itemId,
            string itemName,
            string description)
        {
            RewardConfig config = GetOrCreateReward(rewardId);
            config.displayName = displayName;
            config.description = description;
            config.onceFlagKey = onceFlag;
            config.isFirstClearOnly = true;
            config.rewards = new List<RewardEntry>
            {
                ItemReward(itemId, itemName, 1)
            };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static RewardConfig CreateChapterOneBossReward()
        {
            RewardConfig config = GetOrCreateReward("chapter_1_10_clear");
            config.displayName = "1-10 章节结算";
            config.description = "1-10 Boss 首通固定奖励";
            config.onceFlagKey = "chapter_1_10_clear";
            config.isFirstClearOnly = true;
            config.rewards = new List<RewardEntry>
            {
                ItemReward("sword_pill_basic", "剑丸", 1),
                ResourceReward(ResourceType.SpiritStone, "灵石", 120),
                ResourceReward(ResourceType.TalismanPaper, "符纸", 60),
                ResourceReward(ResourceType.Cinnabar, "朱砂", 10),
                ResourceReward(ResourceType.BasicTalismanEmbryo, "初阶符胚", 1),
                ResourceReward(ResourceType.Cultivation, "修为", 20)
            };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static RewardConfig CreateChapterTwoBossReward()
        {
            RewardConfig config = GetOrCreateReward("boss_2_10_clear");
            config.displayName = "2-10 Boss 结算";
            config.description = "2-10 Boss 首通固定奖励";
            config.onceFlagKey = "boss_2_10_clear";
            config.isFirstClearOnly = true;
            config.rewards = new List<RewardEntry>
            {
                ItemReward("bronze_seal_basic", "青铜法印", 1),
                ResourceReward(ResourceType.BasicTalismanEmbryo, "初阶符胚", 1),
                ResourceReward(ResourceType.SpiritStone, "灵石", 80),
                ResourceReward(ResourceType.TalismanPaper, "符纸", 40),
                ResourceReward(ResourceType.Cinnabar, "朱砂", 6),
                ResourceReward(ResourceType.Cultivation, "修为", 10)
            };
            EditorUtility.SetDirty(config);
            return config;
        }

        private static RewardConfig GetOrCreateReward(string rewardId)
        {
            string path = $"{RewardRoot}/{rewardId}.asset";
            RewardConfig config = AssetDatabase.LoadAssetAtPath<RewardConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<RewardConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.rewardId = rewardId;
            config.sourceType = CatalogSourceType.Verified;
            config.isDebugOnly = false;
            config.isDeprecated = false;
            return config;
        }

        private static RewardEntry ItemReward(string itemId, string displayName, int amount)
        {
            return new RewardEntry
            {
                rewardType = RewardType.Item,
                itemId = itemId,
                displayName = displayName,
                amount = amount
            };
        }

        private static RewardEntry ResourceReward(ResourceType type, string displayName, int amount)
        {
            return new RewardEntry
            {
                rewardType = RewardType.Resource,
                resourceType = type,
                displayName = displayName,
                amount = amount
            };
        }

        private static void CreateTutorialGuideConfig(IReadOnlyDictionary<string, RewardConfig> rewards)
        {
            const string path = "Assets/_Game/Resources/CoreLoop/TutorialGuideConfig_Fix03.asset";
            TutorialGuideConfig config = AssetDatabase.LoadAssetAtPath<TutorialGuideConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<TutorialGuideConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.configId = "fix03_chapter_one_guide";
            config.rows = new List<TutorialGuideRow>
            {
                new()
                {
                    levelId = "1-1",
                    trigger = TutorialGuideTrigger.PreBattle,
                    onceFlag = "tutorial_prebattle_1_1_energy_stone",
                    consumeOnce = true,
                    panelTitle = "聚能石说明",
                    panelSubject = "聚能石 / 聚灵石",
                    panelDescription = "聚能石会为附近符箓提供灵力。\n只有获得灵力的符箓才能发动。\n请让火符处在聚能石供能范围内。",
                    blockFlowWhenRuntimeInventorySyncFails = false
                },
                FixedGuide("1-2", "fixed_reward_1_2_thunder_talisman", "雷符", rewards["fixed_tutorial_1_2"]),
                FixedGuide("1-4", "fixed_reward_1_4_shield_talisman", "护身符", rewards["fixed_tutorial_1_4"]),
                FixedGuide("1-5", "fixed_reward_1_5_purify_talisman", "净化符", rewards["fixed_tutorial_1_5"]),
                FixedGuide("1-6", "fixed_reward_1_6_soul_suppress_talisman", "镇魂符", rewards["fixed_tutorial_1_6"]),
                FixedGuide("1-9", "fixed_reward_1_9_chain_thunder_talisman", "连锁雷符", rewards["fixed_tutorial_1_9"]),
                new()
                {
                    levelId = "1-10",
                    trigger = TutorialGuideTrigger.BossClear,
                    onceFlag = "chapter_1_10_clear",
                    consumeOnce = true,
                    markChapterOneBossClear = true,
                    highestClearedLevelIdOnComplete = "1-10",
                    panelTitle = "1-10 Boss 已击破",
                    panelSubject = "章节奖励",
                    panelDescription = "领取章节结算奖励后完成 1-10 主线试炼。",
                    rewardConfig = rewards["chapter_1_10_clear"],
                    blockFlowWhenRuntimeInventorySyncFails = false
                }
            };
            EditorUtility.SetDirty(config);
        }

        private static TutorialGuideRow FixedGuide(
            string levelId,
            string onceFlag,
            string itemName,
            RewardConfig reward)
        {
            return new TutorialGuideRow
            {
                levelId = levelId,
                trigger = TutorialGuideTrigger.RoundWin,
                onceFlag = onceFlag,
                consumeOnce = true,
                panelTitle = $"获得：{itemName}",
                panelSubject = $"{itemName} x1",
                panelDescription = reward.description,
                rewardConfig = reward,
                blockFlowWhenRuntimeInventorySyncFails = true
            };
        }

        private static void MigrateStageConfig(IReadOnlyDictionary<string, RewardConfig> rewards)
        {
            V02RunConfig runConfig = AssetDatabase.LoadAssetAtPath<V02RunConfig>(RunConfigPath);
            if (runConfig?.rounds == null || runConfig.rounds.Count < 10)
            {
                throw new InvalidOperationException("Verified RunConfig_V02_15Min is missing or has fewer than 10 rounds.");
            }

            Dictionary<string, EnemyDefinition> sourceByStage = runConfig.rounds
                .Where(round => round != null && round.enemy != null)
                .ToDictionary(round => round.levelId, round => round.enemy, StringComparer.Ordinal);

            EnemyDefinition common = sourceByStage["1-1"];
            EnemyDefinition shield = sourceByStage["1-2"];
            EnemyDefinition pressure = sourceByStage["1-3"];
            EnemyDefinition poison = sourceByStage["1-4"];
            EnemyDefinition seal = sourceByStage["1-5"];
            EnemyDefinition disrupt = sourceByStage["1-6"];
            EnemyDefinition boss = sourceByStage["1-10"];

            Dictionary<string, EnemyDefinition> stageEnemies = new(StringComparer.Ordinal)
            {
                ["1-1"] = CreateStageEnemy(common, "1-1", "common_restless_imp_1_1", "游灯小祟", EnemyType.Ghost, 72, 6, 2.8f, "普通小怪", "普通攻击", "把火符放到聚灵石供能范围内。", Array.Empty<CounterTag>(), null, "1-1 low pressure energy tutorial"),
                ["1-2"] = CreateStageEnemy(common, "1-2", "common_restless_imp_1_2", "游灯小祟·复习", EnemyType.Ghost, 96, 8, 2.6f, "普通小怪加强", "普通攻击加强", "复习基础输出。下一关会出现护盾。", Array.Empty<CounterTag>(), null, "1-2 reused common imp, stronger hp/damage"),
                ["1-3"] = CreateStageEnemy(shield, "1-3", "shield_stone_talisman_guard_1_3", "石甲符卫", EnemyType.Ghost, 110, 7, 2.8f, "低强度护盾", "周期护盾", "敌人有护盾，雷符可破盾。", new[] { CounterTag.Shield }, CloneStageSkills(shield, "1-3"), "1-3 shield tutorial"),
                ["1-4"] = CreateStageEnemy(shield, "1-4", "shield_stone_talisman_guard_1_4", "石甲符卫·复习", EnemyType.Ghost, 135, 8, 2.7f, "护盾复习", "更厚护盾", "护盾略厚，复习破盾和输出。", new[] { CounterTag.Shield }, CloneStageSkills(shield, "1-4"), "1-4 reused shield guard review"),
                ["1-5"] = CreateStageEnemy(pressure, "1-5", "pressure_bronze_claw_wight_1_5", "铜爪压阵怪", EnemyType.Ghost, 140, 13, 2.2f, "承压近战", "高压普攻", "护身符可以明显降低承压风险。", new[] { CounterTag.Charge }, null, "1-5 pressure tutorial, reused configured prototype"),
                ["1-6"] = CreateStageEnemy(poison, "1-6", "debuff_cinder_poison_sprite_1_6", "灰火毒祟", EnemyType.Ghost, 130, 8, 2.8f, "毒火负面", "低频毒/燃", "净化符可以清除毒、燃烧等负面状态。", new[] { CounterTag.Poison, CounterTag.Burn }, CloneStageSkills(poison, "1-6"), "1-6 debuff tutorial"),
                ["1-7"] = CreateStageEnemy(disrupt, "1-7", "disrupt_soul_seal_ghost_1_7", "摄灵封符鬼", EnemyType.Ghost, 135, 8, 2.8f, "低频偷灵/封符", "偷灵与封符", "镇魂符可以反制偷灵、封符等异常干扰。", new[] { CounterTag.StealEnergy, CounterTag.Seal }, CloneDisruptSkills(disrupt, seal, "1-7"), "1-7 disruption tutorial"),
                ["1-8"] = CreateStageEnemy(disrupt, "1-8", "disrupt_soul_seal_ghost_1_8", "摄灵封符鬼·复习", EnemyType.Ghost, 155, 9, 2.65f, "异常干扰复习", "偷灵与封符频率略高", "异常干扰频率略高，但不会形成硬卡。", new[] { CounterTag.StealEnergy, CounterTag.Seal }, CloneDisruptSkills(disrupt, seal, "1-8"), "1-8 reused disruption review"),
                ["1-9"] = CreateStageEnemy(common, "1-9", "boss_precheck_imp_guard_1_9", "游灯小祟 + 石甲符卫", EnemyType.Ghost, 175, 9, 2.45f, "Boss前综合小测", "多目标 + 低强度护盾", "多只游灯小祟配合低强度石甲符卫，给连锁雷符发挥空间。", new[] { CounterTag.Group, CounterTag.Shield }, CloneStageSkills(shield, "1-9"), "1-9 reuses common imp and shield guard"),
                ["1-10"] = CreateStageEnemy(boss, "1-10", "boss_array_breaking_warlord_1_10", "破阵妖将", EnemyType.Boss, 240, 12, 2.8f, "第一章 Boss 综合题", "护盾 / 召唤 / 封符综合", "第一章综合 Boss，不做数值墙。", new[] { CounterTag.Shield, CounterTag.Group, CounterTag.Seal, CounterTag.StealEnergy, CounterTag.Boss }, CloneStageSkills(boss, "1-10"), "1-10 chapter boss")
            };

            Dictionary<string, EnemyGroupConfig> chapterOneGroups = new(StringComparer.Ordinal);
            Dictionary<string, EnemyGroupConfig> chapterTwoGroups = new(StringComparer.Ordinal);
            for (int i = 0; i < 10; i++)
            {
                string chapterOneStageId = $"1-{i + 1}";
                string chapterTwoStageId = $"2-{i + 1}";
                chapterOneGroups[chapterOneStageId] = CreateGroup(chapterOneStageId, stageEnemies[chapterOneStageId]);
                chapterTwoGroups[chapterTwoStageId] = CreateGroup(chapterTwoStageId, runConfig.rounds[i].enemy);
            }

            for (int i = 0; i < runConfig.rounds.Count; i++)
            {
                V02RoundConfig round = runConfig.rounds[i];
                ConfigureStage(
                    round,
                    "1",
                    i + 1,
                    chapterOneGroups[round.levelId],
                    ResolveReward(round.levelId, rewards),
                    i == 9 ? StageType.Boss : StageType.Tutorial,
                    i == 9 ? StageWinAction.ChapterClearReward : StageWinAction.NextStage,
                    i < 9 ? $"1-{i + 2}" : "2-1",
                    autoAdvance: i < 9,
                    stopBeforeBoss: false);
            }

            runConfig.chapterTwoRounds = new List<V02RoundConfig>();
            for (int i = 0; i < 10; i++)
            {
                V02RoundConfig source = runConfig.rounds[i];
                int stageNumber = i + 1;
                string stageId = $"2-{stageNumber}";
                V02RoundConfig round = CloneRound(source);
                round.levelId = stageId;
                round.roundIndex = stageNumber;
                round.roundTitle = stageNumber == 10 ? "2-10 Boss" : $"2-{stageNumber} 巡行";
                round.teachingGoal = stageNumber < 10
                    ? "自动巡行主线：验证培养后的阵容强度。"
                    : source.teachingGoal;
                round.preBattleHint = stageNumber < 10
                    ? "巡行中会自动开战，普通关胜利后继续前进。"
                    : "前方煞气聚阵，请先查看敌情并整备背包。";
                ConfigureStage(
                    round,
                    "2",
                    stageNumber,
                    chapterTwoGroups[stageId],
                    stageNumber == 10 ? rewards["boss_2_10_clear"] : null,
                    stageNumber == 10 ? StageType.Boss : StageType.IdleNormal,
                    stageNumber == 9 ? StageWinAction.StopBeforeBoss :
                    stageNumber == 10 ? StageWinAction.ShowHome : StageWinAction.NextStage,
                    stageNumber < 10 ? $"2-{stageNumber + 1}" : string.Empty,
                    autoAdvance: stageNumber < 10,
                    stopBeforeBoss: stageNumber == 9);
                runConfig.chapterTwoRounds.Add(round);
            }

            EditorUtility.SetDirty(runConfig);
        }

        private static RewardConfig ResolveReward(
            string stageId,
            IReadOnlyDictionary<string, RewardConfig> rewards)
        {
            string key = stageId switch
            {
                "1-2" => "fixed_tutorial_1_2",
                "1-4" => "fixed_tutorial_1_4",
                "1-5" => "fixed_tutorial_1_5",
                "1-6" => "fixed_tutorial_1_6",
                "1-9" => "fixed_tutorial_1_9",
                "1-10" => "chapter_1_10_clear",
                _ => string.Empty
            };
            return !string.IsNullOrEmpty(key) ? rewards[key] : null;
        }

        private static void ConfigureStage(
            V02RoundConfig round,
            string chapterId,
            int stageNumber,
            EnemyGroupConfig group,
            RewardConfig reward,
            StageType stageType,
            StageWinAction winAction,
            string nextStageId,
            bool autoAdvance,
            bool stopBeforeBoss)
        {
            round.stageConfigVersion = 1;
            round.chapterId = chapterId;
            round.roundIndex = stageNumber;
            round.nextStageId = nextStageId;
            round.stageType = stageType;
            round.enemyGroup = group;
            round.rewardConfig = reward;
            round.tutorialGuideId = reward != null ? reward.onceFlagKey : string.Empty;
            round.onWinAction = winAction;
            round.onLoseAction = StageLoseAction.RetrySameStage;
            round.autoAdvance = autoAdvance;
            round.allowBackpackEdit = true;
            round.stopBeforeBoss = stopBeforeBoss;
            round.benchmarkTargetId = round.levelId;
            round.isBossRound = stageType == StageType.Boss;
        }

        private static V02RoundConfig CloneRound(V02RoundConfig source)
        {
            return new V02RoundConfig
            {
                levelId = source.levelId,
                roundIndex = source.roundIndex,
                roundTitle = source.roundTitle,
                enemy = source.enemy,
                intendedRole = source.intendedRole,
                teachingGoal = source.teachingGoal,
                preBattleHint = source.preBattleHint,
                recommendedCounterTags = source.recommendedCounterTags != null
                    ? new List<CounterTag>(source.recommendedCounterTags)
                    : new List<CounterTag>(),
                isBossRound = source.isBossRound,
                targetDurationMin = source.targetDurationMin,
                targetDurationMax = source.targetDurationMax,
                targetHpLossMin = source.targetHpLossMin,
                targetHpLossMax = source.targetHpLossMax,
                strongCounterExpectedDuration = source.strongCounterExpectedDuration,
                neutralExpectedDuration = source.neutralExpectedDuration,
                badBuildExpectedDuration = source.badBuildExpectedDuration,
                expectedPlayerHpRemainStrongCounter = source.expectedPlayerHpRemainStrongCounter,
                expectedPlayerHpRemainNeutral = source.expectedPlayerHpRemainNeutral,
                expectedPlayerHpRemainBadBuild = source.expectedPlayerHpRemainBadBuild,
                benchmarkRule = source.benchmarkRule,
                benchmarkTargets = source.benchmarkTargets != null
                    ? new List<V02BuildBenchmarkTargetRow>(source.benchmarkTargets)
                    : new List<V02BuildBenchmarkTargetRow>()
            };
        }

        private static EnemyGroupConfig CreateGroup(string stageId, EnemyDefinition enemy)
        {
            string safeName = stageId.Replace("-", "_");
            string path = $"{EnemyGroupRoot}/enemy_group_{safeName}.asset";
            EnemyGroupConfig group = AssetDatabase.LoadAssetAtPath<EnemyGroupConfig>(path);
            if (group == null)
            {
                group = ScriptableObject.CreateInstance<EnemyGroupConfig>();
                AssetDatabase.CreateAsset(group, path);
            }

            group.enemyGroupId = $"enemy_group_{safeName}";
            group.displayName = $"{stageId} 怪物组";
            group.sourceType = CatalogSourceType.Verified;
            group.isDebugOnly = false;
            group.isDeprecated = false;
            group.entries = new List<EnemyGroupEntry>
            {
                new()
                {
                    enemy = enemy,
                    enemyCount = 1,
                    spawnOrder = 0,
                    hpMultiplier = 1f,
                    attackMultiplier = 1f,
                    attackSpeedMultiplier = 1f,
                    shieldMultiplier = 1f,
                    skillFrequencyMultiplier = 1f
                }
            };
            EditorUtility.SetDirty(group);
            return group;
        }

        private static EnemyDefinition CreateStageEnemy(
            EnemyDefinition source,
            string stageId,
            string enemyId,
            string displayName,
            EnemyType enemyType,
            int maxHp,
            int attackDamage,
            float attackInterval,
            string weaknessText,
            string dangerText,
            string recommendedBuildText,
            IEnumerable<CounterTag> weaknessTags,
            List<EnemySkillDefinition> skills,
            string archetype)
        {
            string safeName = stageId.Replace("-", "_");
            string path = $"{StageEnemyRoot}/{safeName}_{enemyId}.asset";
            EnemyDefinition enemy = AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (enemy == null)
            {
                enemy = InstantiateOrCreate(source);
                AssetDatabase.CreateAsset(enemy, path);
            }
            else
            {
                EditorUtility.CopySerialized(source, enemy);
            }

            enemy.name = $"{safeName}_{enemyId}";
            enemy.enemyId = enemyId;
            enemy.displayName = displayName;
            enemy.enabled = true;
            enemy.enemyType = enemyType;
            enemy.maxHp = maxHp;
            enemy.attackDamage = attackDamage;
            enemy.attackInterval = attackInterval;
            enemy.weaknessText = weaknessText;
            enemy.dangerText = dangerText;
            enemy.recommendedBuildText = recommendedBuildText;
            enemy.enemyClass = "ChapterOneGuide";
            enemy.enemyArchetype = archetype;
            enemy.intentText = dangerText;
            enemy.recommendedCounterText = recommendedBuildText;
            enemy.weaknessTags = weaknessTags?.ToList() ?? new List<CounterTag>();
            enemy.skills = skills ?? new List<EnemySkillDefinition>();
            enemy.sourceType = CatalogSourceType.Verified;
            enemy.isDebugOnly = false;
            enemy.isDeprecated = false;
            EditorUtility.SetDirty(enemy);
            return enemy;
        }

        private static EnemyDefinition InstantiateOrCreate(EnemyDefinition source)
        {
            return source != null
                ? UnityEngine.Object.Instantiate(source)
                : ScriptableObject.CreateInstance<EnemyDefinition>();
        }

        private static List<EnemySkillDefinition> CloneStageSkills(EnemyDefinition source, string stageId)
        {
            List<EnemySkillDefinition> result = new();
            if (source?.skills == null)
            {
                return result;
            }

            foreach (EnemySkillDefinition skill in source.skills)
            {
                EnemySkillDefinition clone = CloneSkill(skill, stageId);
                ApplySkillStageNumbers(clone, stageId);
                if (clone != null)
                {
                    result.Add(clone);
                }
            }

            return result;
        }

        private static List<EnemySkillDefinition> CloneDisruptSkills(
            EnemyDefinition disrupt,
            EnemyDefinition seal,
            string stageId)
        {
            List<EnemySkillDefinition> result = new();
            AddSkillOfType(result, disrupt, EnemySkillType.StealEnergy, stageId);
            AddSkillOfType(result, seal, EnemySkillType.SealRowOrColumn, stageId);
            return result;
        }

        private static void AddSkillOfType(
            List<EnemySkillDefinition> destination,
            EnemyDefinition source,
            EnemySkillType type,
            string stageId)
        {
            EnemySkillDefinition sourceSkill = source?.skills?.FirstOrDefault(skill => skill != null && skill.skillType == type);
            if (sourceSkill == null)
            {
                return;
            }

            EnemySkillDefinition clone = CloneSkill(sourceSkill, stageId);
            ApplySkillStageNumbers(clone, stageId);
            destination.Add(clone);
        }

        private static EnemySkillDefinition CloneSkill(EnemySkillDefinition source, string stageId)
        {
            if (source == null)
            {
                return null;
            }

            string safeName = $"{stageId.Replace("-", "_")}_{source.skillId}";
            string path = $"{StageSkillRoot}/{safeName}.asset";
            EnemySkillDefinition skill = AssetDatabase.LoadAssetAtPath<EnemySkillDefinition>(path);
            if (skill == null)
            {
                skill = UnityEngine.Object.Instantiate(source);
                AssetDatabase.CreateAsset(skill, path);
            }
            else
            {
                EditorUtility.CopySerialized(source, skill);
            }

            skill.name = safeName;
            skill.skillId = $"{stageId}_{source.skillId}";
            EditorUtility.SetDirty(skill);
            return skill;
        }

        private static void ApplySkillStageNumbers(EnemySkillDefinition skill, string levelId)
        {
            if (skill == null)
            {
                return;
            }

            switch (levelId)
            {
                case "1-3" when skill.skillType == EnemySkillType.GainShield:
                    skill.initialDelay = 4f;
                    skill.cooldown = 11f;
                    skill.value = 22;
                    break;
                case "1-4" when skill.skillType == EnemySkillType.GainShield:
                    skill.initialDelay = 3.5f;
                    skill.cooldown = 9f;
                    skill.value = 38;
                    break;
                case "1-6" when skill.skillType is EnemySkillType.ApplyPoison or EnemySkillType.ApplyBurn:
                    skill.initialDelay = 5f;
                    skill.cooldown = 11f;
                    skill.value = Mathf.Max(3, Mathf.RoundToInt(skill.value * 0.65f));
                    break;
                case "1-7" when skill.skillType is EnemySkillType.StealEnergy or EnemySkillType.SealRowOrColumn:
                    skill.initialDelay = 5f;
                    skill.cooldown = 12f;
                    skill.duration = Mathf.Max(2, skill.duration);
                    break;
                case "1-8" when skill.skillType is EnemySkillType.StealEnergy or EnemySkillType.SealRowOrColumn:
                    skill.initialDelay = 4f;
                    skill.cooldown = 9f;
                    skill.duration = Mathf.Max(2, skill.duration);
                    break;
                case "1-9" when skill.skillType == EnemySkillType.GainShield:
                    skill.initialDelay = 4f;
                    skill.cooldown = 12f;
                    skill.value = 26;
                    break;
            }

            EditorUtility.SetDirty(skill);
        }

        private static List<T> LoadAssets<T>(string searchRoot) where T : UnityEngine.Object
        {
            List<T> result = new();
            foreach (string guid in AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { searchRoot }))
            {
                T asset = AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                {
                    result.Add(asset);
                }
            }

            return result;
        }

        private static void EnsureFolder(string path)
        {
            string normalized = path.Replace('\\', '/');
            string[] parts = normalized.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
#endif
