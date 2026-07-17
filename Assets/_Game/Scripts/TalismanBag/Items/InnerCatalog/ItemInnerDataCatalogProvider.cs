using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Detail;
using UnityEngine;

namespace TalismanBag.Items.InnerCatalog
{
    public sealed class ItemInnerDataCatalogProvider : MonoBehaviour, IItemDetailViewModelProvider
    {
        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool readsFormalBattleObject;
        [SerializeField] private bool readsFormalSaveData;
        [SerializeField] private bool writesFormalSystem;

        private readonly List<ItemDetailListEntry> listEntries = new();
        private readonly Dictionary<string, ItemDetailViewModel> viewModelsById = new();
        private bool initialized;

        public bool DevOnly => devOnly;
        public bool ReadsFormalBattleObject => readsFormalBattleObject;
        public bool ReadsFormalSaveData => readsFormalSaveData;
        public bool WritesFormalSystem => writesFormalSystem;
        public int CatalogCount => ItemInnerDataCatalog.AllItems.Count;

        public IReadOnlyList<ItemDetailListEntry> GetItemList()
        {
            EnsureInitialized();
            return listEntries;
        }

        public ItemDetailViewModel GetDetailViewModel(string itemId)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(itemId)
                && viewModelsById.TryGetValue(itemId, out ItemDetailViewModel model))
            {
                return model;
            }

            return viewModelsById.Values.FirstOrDefault();
        }

        public IReadOnlyList<ItemInnerDataDefinition> GetCatalogItems()
        {
            return ItemInnerDataCatalog.AllItems;
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            listEntries.Clear();
            viewModelsById.Clear();

            foreach (ItemInnerDataDefinition item in ItemInnerDataCatalog.AllItems)
            {
                listEntries.Add(new ItemDetailListEntry(item.itemId, item.displayName));
                viewModelsById[item.itemId] = BuildViewModel(item);
            }
        }

        private static ItemDetailViewModel BuildViewModel(ItemInnerDataDefinition item)
        {
            ItemDetailStatusFlags flags = new()
            {
                isLit = false,
                isDirectLit = false,
                isRelayLit = false,
                coreEffectUnlocked = false,
                coreEffectActive = false,
                isOnArrayBonusCell = false,
                isArrayBonusActive = false,
                countedInBuild = false,
                inputLevel = 1,
                resolvedLevel = 1,
                itemLevel = 1,
                unlockedCoreEffectCount = 0,
                activeCoreEffectCount = 0,
                currentAwakeningNodeLevel = 0,
                nextAwakeningNodeLevel = 10
            };

            string lightingStatus = item.isLightingSource
                ? "点亮来源：聚念石是第一版唯一来源；本包不执行 litRangeCells 计算"
                : "目录状态：未接入点亮计算；点亮后才可发动并计入 Build";
            string buildPreviewText = item.isLightingSource
                ? "聚念石只提供点亮来源身份，不计入法门 / 器类 Build"
                : "目录只记录 tag；真实 Build 统计后续只统计已点亮道具";

            ItemDetailViewModel model = new()
            {
                itemId = item.itemId,
                displayItemName = item.displayName,
                displayRarityName = item.displayRarityName,
                displayFaMenName = item.FaMenDisplayName,
                displayQiLeiName = item.QiLeiDisplayName,
                displayShapeName = $"{item.shapeId} · {item.defaultLocalCells.Count}格",
                displayItemPower = item.itemPower,
                displayTriggerText = item.triggerText,
                displayLightingStatusText = lightingStatus,
                displayAwakeningStatusText = item.awakeningPreview,
                displayArrayBonusStatusText = "阵位点预览：阵眼上下左右四格可强化已点亮道具；本包不判断占位",
                statusFlags = flags,
                displayOrangeAffix = item.orangeAffixPreview,
                displayFlavorText = item.flavorText,
                iconPlaceholderKey = item.iconPlaceholderKey,
                rarityColorKey = item.rarityDefault.ToStableKey()
            };

            model.displayPrimaryStats.Add(new ItemDetailStatLine("ItemId", item.itemId, "稳定目录 id"));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("itemFamily", item.ItemFamilyKey, "内层目录 family tag"));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("faMenTag", item.FaMenKey, item.FaMenDisplayName));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("qiLeiTag", item.QiLeiKey, item.QiLeiDisplayName));
            foreach (ItemInnerStatLine stat in item.primaryStats)
            {
                model.displayPrimaryStats.Add(new ItemDetailStatLine(stat.label, stat.value, stat.hint));
            }

            model.displayPrimaryStats.Add(new ItemDetailStatLine("shapeCells", item.FormatCells(), "defaultLocalCells / shapeCells"));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("coreCellLocal", ItemInnerDataDefinition.FormatCell(item.coreCellLocal), "只读目录字段"));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("allowedRarities", item.FormatAllowedRarities(), "本包不 roll 品阶"));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("lightingSource", item.isLightingSource ? "true" : "false", "聚念石唯一为 true"));

            model.displayFixedAffixes.Add(new ItemDetailTextLine("固定词条预览", item.fixedAffixPreview, "catalog"));
            model.displayRandomAffixes.Add(new ItemDetailTextLine("随机词条预览", item.randomAffixPreview, "catalog"));
            model.displayBasicEffects.Add(new ItemDetailTextLine("基础效果", item.basicEffectText, "catalog"));
            model.displayCoreEffects.Add(new ItemDetailTextLine("核心效果预览", item.coreEffectPreviewText, "catalog"));
            model.displayFaMenBuilds.Add(new ItemDetailBuildPreview(
                item.FaMenDisplayName,
                item.isLightingSource ? "不计入" : "0/4/6 未计算",
                buildPreviewText,
                false));
            model.displayQiLeiBuilds.Add(new ItemDetailBuildPreview(
                item.QiLeiDisplayName,
                item.isLightingSource ? "不计入" : "0/2/4 未计算",
                buildPreviewText,
                false));
            model.displayPlacementTips.Add(new ItemDetailTextLine("推荐摆放", item.placementHint, "catalog"));

            model.lightingPreview = new ItemLightingPreview
            {
                isLit = false,
                isDirectLit = false,
                isRelayLit = false,
                isOnArrayBonusCell = false,
                isArrayBonusActive = false,
                previewText = lightingStatus
            };
            model.buildPreview = new ItemBuildPreview
            {
                countedInBuild = false,
                faMenPreviewText = buildPreviewText,
                qiLeiPreviewText = buildPreviewText
            };
            model.awakeningPreview = new ItemAwakeningPreview
            {
                inputLevel = 1,
                resolvedLevel = 1,
                itemLevel = 1,
                coreEffectUnlocked = false,
                coreEffectActive = false,
                unlockedCoreEffectCount = 0,
                activeCoreEffectCount = 0,
                currentNodeText = item.awakeningPreview,
                nextNodeText = "后续包只读接入开窍预览，不接正式养成",
                unlockedNodeLevelsText = "None",
                activeNodeLevelsText = "None",
                unlockedCoreEffectIdsText = "None",
                activeCoreEffectIdsText = "None",
                lockedCoreEffectIdsText = "None",
                validationErrorsText = "None",
                readOnlyInputText = "Catalog default: inputLevel=1, resolvedLevel=1, no formal upgrade input."
            };
            model.acquirePreview = new ItemAcquirePreview
            {
                sourceHint = "来源预留：本包不接掉落 / 奖励 / 获得"
            };
            model.upgradePreview = new ItemUpgradePreview
            {
                upgradeHint = "升级预留：本包不接升级、洗词条或正式存档",
                raritySlotHint = $"默认品阶：{item.rarityDefault.ToStableKey()}",
                affixSlotHint = "固定 / 随机 / 橙色专属均为目录预览"
            };
            model.battleEffectPreview = new ItemBattleEffectPreview
            {
                basicEffectActive = false,
                coreEffectActive = false,
                battleStateText = "目录只读：未接战斗解析、战斗桥接或战斗结算"
            };

            return model;
        }
    }
}
