#if UNITY_EDITOR
using System;
using System.Linq;
using TalismanBag.Enemies;
using TalismanBag.V02.CoreLoop.Boss;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class Task07BossConfigMigration
    {
        private const string RunConfigPath =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset";
        private const string BossRoot =
            "Assets/_Game/Resources/CoreLoop/BossConfigs";
        private const string ChapterOneBossPath =
            BossRoot + "/boss_1_10_array_breaking_warlord.asset";
        private const string ChapterTwoBossPath =
            BossRoot + "/boss_2_10_formation_breaker.asset";

        public static void ExecuteBatch()
        {
            try
            {
                RunMigration();
                Debug.Log("[StageConfigPanel01][Task07] MIGRATION_SUCCESS");
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
            Require(runConfig?.rounds != null && runConfig.rounds.Count == 10,
                "Chapter 1 must contain 10 configured stages.");
            Require(runConfig?.chapterTwoRounds != null && runConfig.chapterTwoRounds.Count == 10,
                "Chapter 2 must contain 10 configured stages.");

            BossInfoConfig chapterOneBoss = AssetDatabase.LoadAssetAtPath<BossInfoConfig>(ChapterOneBossPath);
            BossInfoConfig chapterTwoBoss = AssetDatabase.LoadAssetAtPath<BossInfoConfig>(ChapterTwoBossPath);
            Require(chapterOneBoss != null && runConfig.rounds[9].bossConfig == chapterOneBoss,
                "1-10 must reference its verified BossConfig.");
            Require(chapterTwoBoss != null && runConfig.chapterTwoRounds[9].bossConfig == chapterTwoBoss,
                "2-10 must reference its verified BossConfig.");
            Require(chapterOneBoss.bossEnemy == runConfig.rounds[9].ResolveEnemyDefinition(),
                "1-10 BossConfig must reference the stage Boss enemy.");
            Require(chapterTwoBoss.bossEnemy == runConfig.chapterTwoRounds[9].ResolveEnemyDefinition(),
                "2-10 BossConfig must reference the stage Boss enemy.");
            Require(runConfig.rounds.Take(9).All(stage => stage.bossConfig == null),
                "Only the 1-10 chapter-one stage may reference a BossConfig.");
            Require(runConfig.chapterTwoRounds.Take(9).All(stage => stage.bossConfig == null),
                "Only the 2-10 chapter-two stage may reference a BossConfig.");
            Require(HasVerifiedPhaseValues(chapterOneBoss) && HasVerifiedPhaseValues(chapterTwoBoss),
                "BossConfig phase values must preserve the verified runtime values.");

            var validation = DataCatalogValidator.Validate(DataCatalog.Collect());
            Require(validation.All(result => result.Level != DataCatalogValidationLevel.Error),
                "DataCatalog contains Error results.");
            Debug.Log($"[StageConfigPanel01][Task07] SMOKE_SUCCESS bosses=2, validation={validation.Count}");
        }

        private static void RunMigration()
        {
            EnsureFolder(BossRoot);
            V02RunConfig runConfig = AssetDatabase.LoadAssetAtPath<V02RunConfig>(RunConfigPath);
            if (runConfig?.rounds == null || runConfig.rounds.Count != 10 ||
                runConfig.chapterTwoRounds == null || runConfig.chapterTwoRounds.Count != 10)
            {
                throw new InvalidOperationException("Verified 1-10/2-10 stage configuration is missing.");
            }

            BossInfoConfig chapterOneBoss = CreateBossConfig(
                ChapterOneBossPath,
                runConfig.rounds[9].ResolveEnemyDefinition(),
                "1-10 Boss\uff1a\u7834\u9635\u5996\u5c06",
                "\u62a4\u76fe\u538b\u5236 / \u5996\u7fa4\u53ec\u5524 / \u9635\u773c\u5c01\u5370",
                "\u7b2c\u4e00\u7ae0\u7efc\u5408 Boss\uff0c\u4f1a\u4f9d\u6b21\u65bd\u52a0\u62a4\u76fe\u3001\u53ec\u5524\u4e0e\u5c01\u5370\u538b\u529b\u3002",
                "\u4f18\u5148\u4fdd\u6301\u6838\u5fc3\u7b26\u7b93\u4f9b\u80fd\uff0c\u5408\u7406\u4f7f\u7528\u7834\u76fe\u3001\u6e05\u7fa4\u4e0e\u51c0\u5316\u624b\u6bb5\u3002",
                "\u524d\u65b9\u662f\u7b2c\u4e00\u7ae0\u7efc\u5408\u8bd5\u70bc\uff0c\u8bf7\u68c0\u67e5\u9635\u5bb9\u540e\u5f00\u6218\u3002");
            BossInfoConfig chapterTwoBoss = CreateBossConfig(
                ChapterTwoBossPath,
                runConfig.chapterTwoRounds[9].ResolveEnemyDefinition(),
                "2-10 Boss\uff1a\u715e\u6c14\u805a\u9635",
                "\u62a4\u76fe\u538b\u5236 / \u5996\u7fa4\u53ec\u5524 / \u9635\u773c\u5c01\u5370",
                "Boss \u4f1a\u8f6e\u6d41\u65bd\u52a0\u62a4\u76fe\u538b\u529b\u3001\u5996\u7fa4\u538b\u529b\u548c\u9635\u773c\u5c01\u5370\u538b\u529b\u3002",
                "\u63a8\u8350\u643a\u5e26\u96f7\u7b26\u3001\u51c0\u5316\u7b26\u3001\u9547\u9b42\u7b26\u3001\u62a4\u8eab\u7b26\uff0c\u5e76\u4fdd\u6301\u6838\u5fc3\u7b26\u7b93\u4f9b\u80fd\u3002",
                "\u524d\u65b9\u715e\u6c14\u805a\u9635\uff0c\u8bf7\u5148\u67e5\u770b\u654c\u60c5\u5e76\u6574\u5907\u80cc\u5305\u3002");

            runConfig.rounds[9].bossConfig = chapterOneBoss;
            runConfig.chapterTwoRounds[9].bossConfig = chapterTwoBoss;
            EditorUtility.SetDirty(runConfig);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static BossInfoConfig CreateBossConfig(
            string path,
            EnemyDefinition enemy,
            string displayName,
            string mechanismTags,
            string mainThreats,
            string recommendedTools,
            string preBattlePrompt)
        {
            if (enemy == null || enemy.enemyType != EnemyType.Boss)
            {
                throw new InvalidOperationException($"BossConfig source enemy is invalid for '{path}'.");
            }

            BossInfoConfig config = AssetDatabase.LoadAssetAtPath<BossInfoConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<BossInfoConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            config.bossId = enemy.enemyId;
            config.bossName = displayName;
            config.bossEnemy = enemy;
            config.mechanismTags = mechanismTags;
            config.mainThreats = mainThreats;
            config.recommendedTools = recommendedTools;
            config.preBattlePrompt = preBattlePrompt;
            config.shieldPhaseMinHpRatio = 0.7f;
            config.summonPhaseMinHpRatio = 0.35f;
            config.firstActionDelay = 4f;
            config.shieldInterval = 5f;
            config.summonInterval = 6f;
            config.sealEyeInterval = 7f;
            config.shieldAmount = 30;
            config.summonDamage = 12;
            config.poisonStack = 1;
            config.sealDuration = 3f;
            config.energyDisruptionDuration = 3f;
            config.sourceType = CatalogSourceType.Verified;
            config.isDebugOnly = false;
            config.isDeprecated = false;
            EditorUtility.SetDirty(config);
            return config;
        }

        private static bool HasVerifiedPhaseValues(BossInfoConfig config)
        {
            return Mathf.Approximately(config.shieldPhaseMinHpRatio, 0.7f) &&
                   Mathf.Approximately(config.summonPhaseMinHpRatio, 0.35f) &&
                   Mathf.Approximately(config.firstActionDelay, 4f) &&
                   Mathf.Approximately(config.shieldInterval, 5f) &&
                   Mathf.Approximately(config.summonInterval, 6f) &&
                   Mathf.Approximately(config.sealEyeInterval, 7f) &&
                   config.shieldAmount == 30 &&
                   config.summonDamage == 12 &&
                   config.poisonStack == 1 &&
                   Mathf.Approximately(config.sealDuration, 3f) &&
                   Mathf.Approximately(config.energyDisruptionDuration, 3f) &&
                   config.sourceType == CatalogSourceType.Verified &&
                   !config.isDebugOnly &&
                   !config.isDeprecated;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Replace('\\', '/').Split('/');
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
