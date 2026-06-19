#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.V02.CoreLoop.Resources;
using TalismanBag.V02.CoreLoop.Rewards;
using TalismanBag.V02.Run;
using UnityEditor;
using UnityEngine;

namespace TalismanBag.V02.Config.EditorTools
{
    public static class Task06DropConfigMigration
    {
        private const string RunConfigPath =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/RunConfigs/RunConfig_V02_15Min.asset";
        private const string ItemRoot =
            "Assets/_Game/ScriptableObjects/TalismanBag/V02/Items";
        private const string DropTableRoot =
            "Assets/_Game/Resources/CoreLoop/DropTables";
        private const string DropTablePath =
            DropTableRoot + "/chapter_2_normal_round_drops.asset";
        private const string IdleDropConfigPath =
            "Assets/_Game/Resources/CoreLoop/IdleDropConfig.asset";

        private static readonly string[] RequiredDropItemIds =
        {
            "basic_talisman_embryo_shard",
            "basic_talisman_page",
            "basic_tool_complete"
        };

        public static void ExecuteBatch()
        {
            try
            {
                RunMigration();
                Debug.Log("[StageConfigPanel01][Task06] MIGRATION_SUCCESS");
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
            RewardDropTable dropTable = AssetDatabase.LoadAssetAtPath<RewardDropTable>(DropTablePath);
            IdleDropConfig idleConfig = AssetDatabase.LoadAssetAtPath<IdleDropConfig>(IdleDropConfigPath);

            Require(runConfig?.chapterTwoRounds != null && runConfig.chapterTwoRounds.Count == 10,
                "Chapter 2 must contain 10 configured stages.");
            Require(dropTable != null && dropTable.tableId == "chapter_2_normal_round_drops",
                "Missing verified chapter-two normal drop table.");
            Require(dropTable.drops != null && dropTable.drops.Count == 6,
                "Chapter-two normal drop table must preserve all 6 verified entries.");
            Require(idleConfig != null && idleConfig.normalStageDropTable == dropTable,
                "IdleDropConfig must reference the verified normal drop table.");
            Require(idleConfig.rollsPerStageClear == 1 && idleConfig.grantOnNormalStageClear,
                "IdleDropConfig must preserve one roll per normal-stage clear.");

            for (int i = 0; i < 9; i++)
            {
                V02RoundConfig stage = runConfig.chapterTwoRounds[i];
                Require(stage != null && stage.levelId == $"2-{i + 1}",
                    $"Missing chapter-two stage 2-{i + 1}.");
                Require(stage.stageType == StageType.IdleNormal && stage.dropTable == dropTable,
                    $"{stage.levelId} must reference the verified idle drop table.");
            }

            Require(runConfig.chapterTwoRounds[9].dropTable == null,
                "2-10 Boss must not use the normal-stage drop table.");

            foreach (string itemId in RequiredDropItemIds)
            {
                TalismanItemDefinition item = LoadItem(itemId);
                Require(item != null && item.canDrop && !item.isDeprecated && !item.isDebugOnly,
                    $"Missing active drop item definition '{itemId}'.");
            }

            IReadOnlyList<DataCatalogValidationResult> validation = DataCatalogValidator.Validate(DataCatalog.Collect());
            Require(validation.All(result => result.Level != DataCatalogValidationLevel.Error),
                "DataCatalog contains Error results.");
            Debug.Log($"[StageConfigPanel01][Task06] SMOKE_SUCCESS stages=9, drops={dropTable.drops.Count}, validation={validation.Count}");
        }

        private static void RunMigration()
        {
            EnsureFolder(ItemRoot);
            EnsureFolder(DropTableRoot);

            CreateDropItem(
                "basic_talisman_embryo_shard",
                "初阶符胚碎片",
                "巡行掉落材料；用于后续合成初阶符胚。",
                ItemRarity.Uncommon);
            CreateDropItem(
                "basic_talisman_page",
                "基础符箓残页",
                "巡行掉落材料；用于后续基础符箓相关成长。",
                ItemRarity.Common);
            CreateDropItem(
                "basic_tool_complete",
                "完整基础道具",
                "巡行低概率完整道具占位定义；当前仅进入统一库存。",
                ItemRarity.Rare);

            RewardDropTable dropTable = CreateDropTable();
            CreateIdleDropConfig(dropTable);
            AttachDropTableToStages(dropTable);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateDropItem(
            string itemId,
            string displayName,
            string description,
            ItemRarity rarity)
        {
            string path = $"{ItemRoot}/{itemId}.asset";
            TalismanItemDefinition item = AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<TalismanItemDefinition>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.itemId = itemId;
            item.displayName = displayName;
            item.enabled = true;
            item.itemType = TalismanItemType.PassiveTool;
            item.width = 1;
            item.height = 1;
            item.baseCooldown = 1f;
            item.baseValue = 0;
            item.description = description;
            item.shortRoleDescription = "巡行掉落库存材料";
            item.rarity = rarity;
            item.canDrop = true;
            item.canUpgrade = false;
            item.unlockChapter = 2;
            item.requiresFormationPower = false;
            item.sourceType = CatalogSourceType.Verified;
            item.isDebugOnly = false;
            item.isDeprecated = false;
            EditorUtility.SetDirty(item);
        }

        private static RewardDropTable CreateDropTable()
        {
            RewardDropTable table = AssetDatabase.LoadAssetAtPath<RewardDropTable>(DropTablePath);
            if (table == null)
            {
                table = ScriptableObject.CreateInstance<RewardDropTable>();
                AssetDatabase.CreateAsset(table, DropTablePath);
            }

            table.tableId = "chapter_2_normal_round_drops";
            table.displayName = "2-1 到 2-9 普通关掉落";
            table.sourceType = CatalogSourceType.Verified;
            table.isDebugOnly = false;
            table.isDeprecated = false;
            table.drops = new List<RewardDropEntry>
            {
                ResourceDrop(ResourceType.SpiritStone, "灵石", 1f, 8, 12),
                ResourceDrop(ResourceType.TalismanPaper, "符纸", 0.5f, 1, 2),
                ResourceDrop(ResourceType.Cinnabar, "朱砂", 0.3f, 1, 1),
                ItemDrop("basic_talisman_embryo_shard", "初阶符胚碎片", 0.15f),
                ItemDrop("basic_talisman_page", "基础符箓残页", 0.2f),
                ItemDrop("basic_tool_complete", "完整基础道具", 0.05f)
            };
            EditorUtility.SetDirty(table);
            return table;
        }

        private static void CreateIdleDropConfig(RewardDropTable dropTable)
        {
            IdleDropConfig config = AssetDatabase.LoadAssetAtPath<IdleDropConfig>(IdleDropConfigPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<IdleDropConfig>();
                AssetDatabase.CreateAsset(config, IdleDropConfigPath);
            }

            config.configId = "chapter_2_idle_drop";
            config.displayName = "第二章巡行掉落";
            config.normalStageDropTable = dropTable;
            config.rollsPerStageClear = 1;
            config.grantOnNormalStageClear = true;
            config.sourceType = CatalogSourceType.Verified;
            config.isDebugOnly = false;
            config.isDeprecated = false;
            EditorUtility.SetDirty(config);
        }

        private static void AttachDropTableToStages(RewardDropTable dropTable)
        {
            V02RunConfig runConfig = AssetDatabase.LoadAssetAtPath<V02RunConfig>(RunConfigPath);
            if (runConfig?.chapterTwoRounds == null || runConfig.chapterTwoRounds.Count != 10)
            {
                throw new InvalidOperationException("Verified chapter-two stage configuration is missing.");
            }

            for (int i = 0; i < runConfig.chapterTwoRounds.Count; i++)
            {
                V02RoundConfig stage = runConfig.chapterTwoRounds[i];
                stage.dropTable = i < 9 ? dropTable : null;
            }

            EditorUtility.SetDirty(runConfig);
        }

        private static TalismanItemDefinition LoadItem(string itemId)
        {
            return AssetDatabase.LoadAssetAtPath<TalismanItemDefinition>($"{ItemRoot}/{itemId}.asset");
        }

        private static RewardDropEntry ResourceDrop(
            ResourceType resourceType,
            string displayName,
            float chance,
            int minAmount,
            int maxAmount)
        {
            return new RewardDropEntry
            {
                chance = chance,
                minAmount = minAmount,
                maxAmount = maxAmount,
                reward = new RewardEntry
                {
                    rewardType = RewardType.Resource,
                    resourceType = resourceType,
                    amount = 1,
                    displayName = displayName
                }
            };
        }

        private static RewardDropEntry ItemDrop(string itemId, string displayName, float chance)
        {
            return new RewardDropEntry
            {
                chance = chance,
                minAmount = 1,
                maxAmount = 1,
                reward = new RewardEntry
                {
                    rewardType = RewardType.Item,
                    itemId = itemId,
                    amount = 1,
                    displayName = displayName
                }
            };
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
