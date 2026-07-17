using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Detail;
using UnityEngine;

namespace TalismanBag.ItemSandbox
{
    public sealed class ItemSandboxDevStubProvider : MonoBehaviour,
        IItemDetailViewModelProvider
    {
        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool readsFormalBattleObject;
        [SerializeField] private bool readsFormalSaveData;
        [SerializeField] private bool writesFormalSystem;

        private readonly List<ItemDetailViewModel> viewModels = new();
        private readonly List<ItemDetailListEntry> listEntries = new();
        private bool initialized;

        public bool DevOnly => devOnly;
        public bool ReadsFormalBattleObject => readsFormalBattleObject;
        public bool ReadsFormalSaveData => readsFormalSaveData;
        public bool WritesFormalSystem => writesFormalSystem;

        public IReadOnlyList<ItemDetailListEntry> GetItemList()
        {
            EnsureInitialized();
            return listEntries;
        }

        public ItemDetailViewModel GetDetailViewModel(string itemId)
        {
            EnsureInitialized();
            ItemDetailViewModel match = viewModels.FirstOrDefault(model =>
                string.Equals(model.itemId, itemId, StringComparison.Ordinal));
            return match ?? viewModels.FirstOrDefault();
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
            viewModels.Clear();
            listEntries.Clear();

            Add(CreateModel(
                "junian_stone",
                "聚念石",
                "青",
                "聚念",
                "石",
                "1格",
                "源石",
                isLit: true,
                isDirectLit: true,
                isRelayLit: false,
                coreUnlocked: false,
                onArrayBonus: false,
                arrayBonusActive: false,
                triggerText: "接入阵眼后给附近道具提供点亮范围；本包只显示状态，不做范围计算。",
                basicEffect: "提供点亮来源预览，不直接解锁核心效果。",
                coreEffect: "核心效果预留：聚念回流。后续养成开窍后再显示。",
                placement: "靠近阵眼，但不可覆盖阵眼石。"));

            Add(CreateModel(
                "fire_talisman",
                "火符",
                "白",
                "离火法",
                "符",
                "1格",
                "斗法小件",
                isLit: true,
                isDirectLit: true,
                isRelayLit: false,
                coreUnlocked: false,
                onArrayBonus: true,
                arrayBonusActive: true,
                triggerText: "已被聚念石点亮，基础效果可发动。",
                basicEffect: "造成小段火符伤害。",
                coreEffect: "核心效果：Lv.10 开窍后解锁离火余烬。",
                placement: "可放在阵脉点旁，已点亮时吃阵位加成。"));

            Add(CreateModel(
                "thunder_talisman",
                "震雷符",
                "蓝",
                "震雷法",
                "符",
                "1格",
                "爆发小件",
                isLit: false,
                isDirectLit: false,
                isRelayLit: false,
                coreUnlocked: true,
                onArrayBonus: true,
                arrayBonusActive: false,
                triggerText: "未点亮，无法发动，也不计入 Build。",
                basicEffect: "点亮后提供短促雷击。",
                coreEffect: "已开窍，但未点亮时核心效果不发动。",
                placement: "需要先让 coreCell 进入点亮范围或被上下左右接亮。"));

            Add(CreateModel(
                "ward_talisman",
                "护身符",
                "绿",
                "中岳法",
                "符",
                "2格",
                "防护件",
                isLit: true,
                isDirectLit: false,
                isRelayLit: true,
                coreUnlocked: true,
                onArrayBonus: false,
                arrayBonusActive: false,
                triggerText: "由相邻已点亮道具接亮，上下左右相邻才成立。",
                basicEffect: "提供护身缓冲。",
                coreEffect: "已点亮 + 已开窍，核心效果可发动。",
                placement: "适合贴着已点亮小件，形成接亮链。"));

            Add(CreateModel(
                "water_talisman",
                "净水符",
                "白",
                "玄水法",
                "符",
                "1格",
                "净化件",
                isLit: false,
                isDirectLit: false,
                isRelayLit: false,
                coreUnlocked: false,
                onArrayBonus: false,
                arrayBonusActive: false,
                triggerText: "未点亮，基础效果和核心效果都不发动。",
                basicEffect: "点亮后提供净化倾向。",
                coreEffect: "核心效果：Lv.10 开窍后解锁净水回护。",
                placement: "先接入聚念石点亮路线，再考虑 Build。"));

            Add(CreateModel(
                "taomu_sword",
                "桃木剑",
                "紫",
                "震雷法",
                "剑",
                "3格",
                "破邪件",
                isLit: true,
                isDirectLit: false,
                isRelayLit: true,
                coreUnlocked: true,
                onArrayBonus: true,
                arrayBonusActive: true,
                triggerText: "已被接亮，且占用阵位点，阵位加成生效。",
                basicEffect: "提供破邪短击。",
                coreEffect: "已点亮 + 已开窍，核心效果可发动。",
                placement: "长条件先确认空间，再让核心格贴近接亮链。"));

            Add(CreateModel(
                "awake_incense",
                "醒符香",
                "绿",
                "离火法",
                "香",
                "2格",
                "养成提示件",
                isLit: true,
                isDirectLit: true,
                isRelayLit: false,
                coreUnlocked: false,
                onArrayBonus: false,
                arrayBonusActive: false,
                triggerText: "已点亮，只发动基础效果。",
                basicEffect: "提供节奏提示与轻量增益预览。",
                coreEffect: "未开窍，核心效果不发动。",
                placement: "不需要抢阵位点，可用于补接亮路线。"));

            Add(CreateModel(
                "furnace_core_stone",
                "炉芯石",
                "橙",
                "中岳法",
                "石",
                "4格",
                "核心件",
                isLit: true,
                isDirectLit: true,
                isRelayLit: false,
                coreUnlocked: true,
                onArrayBonus: false,
                arrayBonusActive: false,
                triggerText: "已点亮且已开窍，但未占阵位点。",
                basicEffect: "提供稳固核心倾向。",
                coreEffect: "核心效果可发动，但当前无阵位加成。",
                placement: "大件先避开阵眼石，再围绕阵位点规划。"));

            Add(CreateModel(
                "mirror_zhaosha",
                "照煞镜",
                "蓝",
                "太白法",
                "镜",
                "2格",
                "照影件",
                isLit: true,
                isDirectLit: false,
                isRelayLit: true,
                coreUnlocked: false,
                onArrayBonus: true,
                arrayBonusActive: true,
                triggerText: "接亮后基础效果生效；阵位加成已生效。",
                basicEffect: "提供照煞反馈预览。",
                coreEffect: "核心效果：Lv.10 开窍后解锁镜照回响。",
                placement: "适合贴阵脉点做已点亮加成样例。"));

            Add(CreateModel(
                "zhongyue_seal",
                "中岳镇守章",
                "紫",
                "中岳法",
                "印",
                "4格",
                "镇守件",
                isLit: false,
                isDirectLit: false,
                isRelayLit: false,
                coreUnlocked: true,
                onArrayBonus: true,
                arrayBonusActive: false,
                triggerText: "占用阵位点但未点亮，阵位加成不生效。",
                basicEffect: "点亮后提供镇守防护。",
                coreEffect: "已开窍，但必须点亮后才发动核心效果。",
                placement: "不要只抢阵位点；未点亮时不计入 Build。"));
        }

        private void Add(ItemDetailViewModel model)
        {
            viewModels.Add(model);
            listEntries.Add(new ItemDetailListEntry(model.itemId, model.displayItemName));
        }

        private static ItemDetailViewModel CreateModel(
            string itemId,
            string name,
            string rarity,
            string faMen,
            string qiLei,
            string shape,
            string itemPower,
            bool isLit,
            bool isDirectLit,
            bool isRelayLit,
            bool coreUnlocked,
            bool onArrayBonus,
            bool arrayBonusActive,
            string triggerText,
            string basicEffect,
            string coreEffect,
            string placement)
        {
            ItemDetailStatusFlags flags = new()
            {
                isLit = isLit,
                isDirectLit = isDirectLit,
                isRelayLit = isRelayLit,
                coreEffectUnlocked = coreUnlocked,
                coreEffectActive = isLit && coreUnlocked,
                isOnArrayBonusCell = onArrayBonus,
                isArrayBonusActive = arrayBonusActive,
                countedInBuild = isLit,
                inputLevel = coreUnlocked ? 10 : 1,
                resolvedLevel = coreUnlocked ? 10 : 1,
                itemLevel = coreUnlocked ? 10 : 1,
                unlockedCoreEffectCount = coreUnlocked ? 1 : 0,
                activeCoreEffectCount = isLit && coreUnlocked ? 1 : 0,
                currentAwakeningNodeLevel = coreUnlocked ? 10 : 0,
                nextAwakeningNodeLevel = coreUnlocked ? 20 : 10
            };

            ItemDetailViewModel model = new()
            {
                itemId = itemId,
                displayItemName = name,
                displayRarityName = rarity,
                displayFaMenName = faMen,
                displayQiLeiName = qiLei,
                displayShapeName = shape,
                displayItemPower = string.Empty,
                displayTriggerText = triggerText,
                displayLightingStatusText = BuildLightingStatus(flags),
                displayAwakeningStatusText = coreUnlocked ? "已开窍：核心效果等待点亮后发动" : "未开窍：核心效果暂不发动",
                displayArrayBonusStatusText = BuildArrayBonusStatus(flags),
                statusFlags = flags,
                displayOrangeAffix = rarity == "橙" ? "道痕：炉火守中。橙色专属当前仅作占位展示。" : string.Empty,
                displayFlavorText = BuildFlavor(name, faMen),
                iconPlaceholderKey = itemId + "_placeholder",
                rarityColorKey = rarity
            };

            model.displayPrimaryStats.Add(new ItemDetailStatLine("核心格", "coreCell 预留", "本包不做真实坐标计算"));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("占格", shape, "用于详情显示，不做放置验证"));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("生效", isLit ? "已参与斗法" : "未生效", "Build 只统计已点亮道具"));

            model.displayFixedAffixes.Add(new ItemDetailTextLine("固定词条", $"{faMen} 线索 +1", "stub"));
            model.displayRandomAffixes.Add(new ItemDetailTextLine("随机词条", "灰盒预览词条，后续 ItemInnerDataCatalog 再落正式池", "stub"));
            model.displayBasicEffects.Add(new ItemDetailTextLine("基础效果", basicEffect, isLit ? "active" : "inactive"));
            model.displayCoreEffects.Add(new ItemDetailTextLine("核心效果", coreEffect, flags.isLit && flags.coreEffectUnlocked ? "active" : "locked"));
            model.displayFaMenBuilds.Add(new ItemDetailBuildPreview(faMen, isLit ? "1/4 预览" : "0/4 未计入", isLit ? "已点亮，计入法门 Build 预览" : "未点亮，不计入法门 Build", isLit));
            model.displayQiLeiBuilds.Add(new ItemDetailBuildPreview(qiLei, isLit ? "1/2 预览" : "0/2 未计入", isLit ? "已点亮，计入器类 Build 预览" : "未点亮，不计入器类 Build", isLit));
            model.displayPlacementTips.Add(new ItemDetailTextLine("推荐摆放", placement, "stub"));

            model.lightingPreview = new ItemLightingPreview
            {
                isLit = flags.isLit,
                isDirectLit = flags.isDirectLit,
                isRelayLit = flags.isRelayLit,
                isOnArrayBonusCell = flags.isOnArrayBonusCell,
                isArrayBonusActive = flags.isArrayBonusActive,
                previewText = model.displayLightingStatusText
            };
            model.buildPreview = new ItemBuildPreview
            {
                countedInBuild = flags.countedInBuild,
                faMenPreviewText = model.displayFaMenBuilds[0].previewText,
                qiLeiPreviewText = model.displayQiLeiBuilds[0].previewText
            };
            model.awakeningPreview = new ItemAwakeningPreview
            {
                inputLevel = flags.inputLevel,
                resolvedLevel = flags.resolvedLevel,
                itemLevel = flags.itemLevel,
                coreEffectUnlocked = flags.coreEffectUnlocked,
                coreEffectActive = flags.coreEffectActive,
                unlockedCoreEffectCount = flags.unlockedCoreEffectCount,
                activeCoreEffectCount = flags.activeCoreEffectCount,
                currentNodeText = coreUnlocked ? "当前：已开窍" : "当前：未开窍",
                nextNodeText = coreUnlocked ? "后续：Lv.20 / Lv.30 / Lv.40 节点预留" : "下一节点：Lv.10 开窍",
                unlockedNodeLevelsText = coreUnlocked ? "Lv.10" : "None",
                activeNodeLevelsText = flags.coreEffectActive ? "Lv.10" : "None",
                unlockedCoreEffectIdsText = coreUnlocked ? $"{itemId}_CORE_01" : "None",
                activeCoreEffectIdsText = flags.coreEffectActive ? $"{itemId}_CORE_01" : "None",
                lockedCoreEffectIdsText = coreUnlocked ? $"{itemId}_CORE_02|{itemId}_CORE_03|{itemId}_CORE_ULT" : $"{itemId}_CORE_01|{itemId}_CORE_02|{itemId}_CORE_03|{itemId}_CORE_ULT",
                validationErrorsText = "None",
                readOnlyInputText = "Dev stub default: preview-only inputLevel/resolvedLevel, no formal upgrade input."
            };
            model.acquirePreview = new ItemAcquirePreview
            {
                sourceHint = "来源提示预留：不接正式掉落 / 奖励。"
            };
            model.upgradePreview = new ItemUpgradePreview
            {
                upgradeHint = "升级预留：不接正式升级或 SaveData。",
                raritySlotHint = "品阶槽预留：" + rarity,
                affixSlotHint = "词条槽预留：固定 / 随机 / 道痕"
            };
            model.battleEffectPreview = new ItemBattleEffectPreview
            {
                basicEffectActive = flags.isLit,
                coreEffectActive = flags.coreEffectActive,
                battleStateText = flags.isLit
                    ? flags.coreEffectUnlocked ? "基础效果与核心效果均可发动" : "基础效果生效，核心效果未开窍"
                    : "未点亮：战斗中不发动，也不计入 Build"
            };

            return model;
        }

        private static string BuildLightingStatus(ItemDetailStatusFlags flags)
        {
            if (!flags.isLit)
            {
                return "未点亮：无法发动，也不计入 Build";
            }

            if (flags.isDirectLit)
            {
                return "已点亮：基础效果生效（聚念石直接点亮预览）";
            }

            if (flags.isRelayLit)
            {
                return "已点亮：由上下左右相邻道具接亮";
            }

            return "已点亮：基础效果生效";
        }

        private static string BuildArrayBonusStatus(ItemDetailStatusFlags flags)
        {
            if (!flags.isOnArrayBonusCell)
            {
                return "阵位加成：未占用阵位点";
            }

            return flags.isArrayBonusActive
                ? "阵位加成：已点亮后生效"
                : "阵位加成：未生效，需先点亮";
        }

        private static string BuildFlavor(string name, string faMen)
        {
            return $"{name} 是旧物鉴定卡中的灰盒样例，当前只服务 {faMen} 方向的字段预览。";
        }
    }
}
