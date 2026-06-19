#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.MainTrial;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class Task09_10QaSmoke
    {
        private const string RunConfigPath =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset";

        public static void VerifyBatch()
        {
            V02RunConfig runConfig = AssetDatabase.LoadAssetAtPath<V02RunConfig>(RunConfigPath);
            Require(runConfig != null, "Missing verified V0.2 RunConfig.");
            Require(runConfig.rounds?.Count == 10, "Chapter 1 stage count must be 10.");
            Require(runConfig.chapterTwoRounds?.Count == 10, "Chapter 2 stage count must be 10.");

            List<V02RoundConfig> stages = new();
            stages.AddRange(runConfig.rounds);
            stages.AddRange(runConfig.chapterTwoRounds);
            Require(stages.Count == 20, "Total stage count must be 20.");
            Require(stages.All(stage => stage != null), "Stage rows must not be null.");
            Require(stages.Select(stage => stage.levelId).SequenceEqual(
                Enumerable.Range(1, 10).Select(index => $"1-{index}")
                    .Concat(Enumerable.Range(1, 10).Select(index => $"2-{index}"))),
                "Stage IDs must contain ordered 1-1..1-10 and 2-1..2-10.");
            Require(stages.All(stage => stage.enemyGroup != null), "Every stage must resolve an EnemyGroupConfig.");
            Require(stages.All(stage => stage.enemyGroup.ResolvePrimaryEnemy() != null), "Every EnemyGroupConfig must resolve an enemy.");
            Require(stages.Count(stage => stage.stopBeforeBoss) == 1, "Exactly one stage must stop before Boss.");

            V02RoundConfig chapterTwoNine = stages.Single(stage => stage.levelId == "2-9");
            V02RoundConfig chapterTwoBoss = stages.Single(stage => stage.levelId == "2-10");
            Require(chapterTwoNine.stopBeforeBoss, "2-9 must be the stopBeforeBoss stage.");
            Require(chapterTwoNine.nextStageId == "2-10", "2-9 must point to 2-10.");
            Require(!chapterTwoBoss.autoAdvance, "2-10 autoAdvance must be false.");
            Require(chapterTwoBoss.isBossRound, "2-10 must be a Boss stage.");

            foreach (V02RoundConfig stage in stages)
            {
                if (stage.rewardConfig != null)
                {
                    Require(!string.IsNullOrWhiteSpace(stage.rewardConfig.rewardId), $"{stage.levelId} has an invalid RewardConfig.");
                }

                if (stage.dropTable != null)
                {
                    Require(!string.IsNullOrWhiteSpace(stage.dropTable.tableId), $"{stage.levelId} has an invalid DropTable.");
                }

                if (stage.bossConfig != null)
                {
                    Require(stage.isBossRound, $"{stage.levelId} references BossConfig but is not a Boss stage.");
                    Require(stage.bossConfig.bossEnemy != null, $"{stage.levelId} BossConfig has no Boss enemy.");
                }
            }

            Require(RewardConfig.LoadById("fixed_tutorial_1_2") != null, "Missing fixed tutorial reward 1-2.");
            Require(RewardConfig.LoadById("fixed_tutorial_1_4") != null, "Missing fixed tutorial reward 1-4.");
            Require(RewardConfig.LoadById("fixed_tutorial_1_5") != null, "Missing fixed tutorial reward 1-5.");
            Require(RewardConfig.LoadById("fixed_tutorial_1_6") != null, "Missing fixed tutorial reward 1-6.");
            Require(RewardConfig.LoadById("fixed_tutorial_1_9") != null, "Missing fixed tutorial reward 1-9.");
            Require(RewardConfig.LoadById("chapter_1_10_clear") != null, "Missing 1-10 Boss chapter reward.");
            Require(RewardConfig.LoadById("boss_2_10_clear") != null, "Missing 2-10 Boss reward.");

            DataCatalog catalog = DataCatalog.Collect();
            TalismanItemDefinition bronzeSeal = catalog.Items.FirstOrDefault(item => item?.itemId == "bronze_seal_basic");
            Require(bronzeSeal != null && !bronzeSeal.isDebugOnly && !bronzeSeal.isDeprecated, "Missing active bronze seal item.");

            IReadOnlyList<DataCatalogValidationResult> validation = DataCatalogValidator.Validate(catalog);
            int errors = validation.Count(result => result.Level == DataCatalogValidationLevel.Error);
            int warnings = validation.Count(result => result.Level == DataCatalogValidationLevel.Warning);
            int infos = validation.Count(result => result.Level == DataCatalogValidationLevel.Info);
            Require(errors == 0, "DataCatalog contains Error results.");
            Require(warnings == 15, $"Expected 15 explainable warnings, found {warnings}.");
            Require(infos == 0, $"Expected 0 info results, found {infos}.");

            RequirePublicQaMethod(typeof(V02RunFlowController), "QaResetCoreLoopSave");
            RequirePublicQaMethod(typeof(V02RunFlowController), "QaStartMainTrialFromOneOne");
            RequirePublicQaMethod(typeof(V02RunFlowController), "QaJumpToChapterOneBoss");
            RequirePublicQaMethod(typeof(V02RunFlowController), "QaJumpToChapterTwoBossReady");
            RequirePublicQaMethod(typeof(V02RunFlowController), "QaGrantRewardById");
            RequirePublicQaMethod(typeof(MainTrialFlowService), "PrepareChapterOneBossForQa");
            RequirePublicQaMethod(typeof(MainTrialFlowService), "PrepareChapterTwoBossForQa");

            Debug.Log(
                $"[StageConfigPanel01][Task09-10] SMOKE_SUCCESS " +
                $"stages={stages.Count}, groups={catalog.EnemyGroups.Count}, " +
                $"errors={errors}, warnings={warnings}, infos={infos}, qaMethods=7");
        }

        private static void RequirePublicQaMethod(Type type, string methodName)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            Require(method != null, $"Missing QA method {type.Name}.{methodName}.");
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
