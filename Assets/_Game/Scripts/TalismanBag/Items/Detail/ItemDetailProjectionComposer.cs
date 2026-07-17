using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.Lighting;
using TalismanBag.Items.Skills;
using UnityEngine;

namespace TalismanBag.Items.Detail
{
    public enum ItemDetailProjectionContextKind
    {
        CatalogPreview = 0,
        PlacedInstance = 1,
        GeneratedInstancePreview = 2
    }

    public sealed class ItemDetailProjectionInput
    {
        public ItemDetailViewModel catalogBaseModel;
        public ItemDetailProjectionContextKind contextKind;
        public string placementId;
        public ItemLightingResolutionResult lightingSnapshot;
        public ItemArrayBonusResolutionResult arrayBonusSnapshot;
        public ItemBuildSynergyResolutionResult buildSnapshot;
        public ItemCoreAwakeningResolutionResult coreAwakeningSnapshot;
        public ItemSkillMonitorResolutionResult skillMonitorSnapshot;
    }

    public static class ItemDetailProjectionComposer
    {
        private const string MissingState = "状态不可用/未接入";
        private const string CatalogPreviewState = "尚未摆放/当前阵局状态不可用";
        private const string InstanceUnavailableState = "实例状态不可用";
        private const string ReservedText = "系统预留";
        private const string UnconfiguredText = "当前版本暂未配置";

        public static ItemDetailViewModel Compose(
            ItemDetailViewModel catalogBaseModel,
            string placementId,
            ItemLightingResolutionResult lightingSnapshot,
            ItemArrayBonusResolutionResult arrayBonusSnapshot,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemCoreAwakeningResolutionResult coreAwakeningSnapshot,
            ItemSkillMonitorResolutionResult skillMonitorSnapshot)
        {
            string normalizedPlacementId = NormalizePlacementId(placementId, catalogBaseModel?.placementId);
            return Compose(
                catalogBaseModel,
                string.IsNullOrWhiteSpace(normalizedPlacementId)
                    ? ItemDetailProjectionContextKind.CatalogPreview
                    : ItemDetailProjectionContextKind.PlacedInstance,
                normalizedPlacementId,
                lightingSnapshot,
                arrayBonusSnapshot,
                buildSnapshot,
                coreAwakeningSnapshot,
                skillMonitorSnapshot);
        }

        public static ItemDetailViewModel Compose(
            ItemDetailViewModel catalogBaseModel,
            ItemDetailProjectionContextKind contextKind,
            string placementId,
            ItemLightingResolutionResult lightingSnapshot,
            ItemArrayBonusResolutionResult arrayBonusSnapshot,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemCoreAwakeningResolutionResult coreAwakeningSnapshot,
            ItemSkillMonitorResolutionResult skillMonitorSnapshot)
        {
            return Compose(new ItemDetailProjectionInput
            {
                catalogBaseModel = catalogBaseModel,
                contextKind = contextKind,
                placementId = placementId,
                lightingSnapshot = lightingSnapshot,
                arrayBonusSnapshot = arrayBonusSnapshot,
                buildSnapshot = buildSnapshot,
                coreAwakeningSnapshot = coreAwakeningSnapshot,
                skillMonitorSnapshot = skillMonitorSnapshot
            });
        }

        public static ItemDetailViewModel Compose(ItemDetailProjectionInput input)
        {
            if (input?.catalogBaseModel == null)
            {
                return null;
            }

            ItemDetailViewModel model = input.catalogBaseModel.Clone();
            string itemId = model.itemId ?? string.Empty;
            ProjectionQueryState query = ResolveQueryState(input, itemId);
            ItemLightingItemResult lighting = query.CanReadPlacementState
                ? FindLighting(input.lightingSnapshot, query.requestedPlacementId)
                : null;
            ItemArrayBonusItemResult arrayBonus = query.CanReadPlacementState
                ? FindArrayBonus(input.arrayBonusSnapshot, query.requestedPlacementId)
                : null;
            ItemBuildSynergyItemResult buildItem = query.CanReadPlacementState
                ? FindBuildItem(input.buildSnapshot, query.requestedPlacementId)
                : null;
            ItemCoreAwakeningItemResult awakening = query.CanReadPlacementState
                ? FindAwakening(input.coreAwakeningSnapshot, query.requestedPlacementId)
                : null;
            ItemSkillMonitorResolutionResult skillMonitor = query.CanReadPlacementState
                ? input.skillMonitorSnapshot
                : null;
            query.FinalizeState(input, lighting, arrayBonus, buildItem, awakening, skillMonitor);

            ApplyStateProjection(model, query, lighting, arrayBonus, buildItem, awakening, skillMonitor);
            model.displayPlayerSections = BuildPlayerSections(model, query, lighting, arrayBonus, input.buildSnapshot, buildItem, awakening, skillMonitor);
            model.displayDebugSections = BuildDebugSections(model, query, lighting, arrayBonus, input.buildSnapshot, buildItem, awakening, skillMonitor);
            return model;
        }

        private static void ApplyStateProjection(
            ItemDetailViewModel model,
            ProjectionQueryState query,
            ItemLightingItemResult lighting,
            ItemArrayBonusItemResult arrayBonus,
            ItemBuildSynergyItemResult buildItem,
            ItemCoreAwakeningItemResult awakening,
            ItemSkillMonitorResolutionResult skillMonitor)
        {
            model.placementId = query.resolvedPlacementId;
            model.statusFlags.placementId = query.resolvedPlacementId;

            if (lighting != null)
            {
                bool basicEffectActive = lighting.isLit && !lighting.isLightingSource;
                model.statusFlags.isLit = lighting.isLit;
                model.statusFlags.isLightingSource = lighting.isLightingSource;
                model.statusFlags.isDirectLit = lighting.isDirectLit;
                model.statusFlags.isRelayLit = lighting.IsRelayLit;
                model.statusFlags.litByItemId = lighting.litByItemId;
                model.statusFlags.litDepth = lighting.litDepth;
                model.statusFlags.basicEffectActive = basicEffectActive;
                model.displayLightingStatusText = LightingPlayerStatus(lighting);
                model.lightingPreview.placementId = query.resolvedPlacementId;
                model.lightingPreview.isLit = lighting.isLit;
                model.lightingPreview.isLightingSource = lighting.isLightingSource;
                model.lightingPreview.isDirectLit = lighting.isDirectLit;
                model.lightingPreview.isRelayLit = lighting.IsRelayLit;
                model.lightingPreview.litByItemId = lighting.litByItemId;
                model.lightingPreview.litDepth = lighting.litDepth;
                model.lightingPreview.basicEffectActive = basicEffectActive;
                model.lightingPreview.previewText = model.displayLightingStatusText;
                model.battleEffectPreview.basicEffectActive = basicEffectActive;
            }
            else
            {
                model.displayLightingStatusText = query.LightingFallbackText;
                model.lightingPreview.previewText = query.LightingFallbackText;
            }

            if (arrayBonus != null)
            {
                model.statusFlags.isOnArrayBonusCell = arrayBonus.isOnArrayBonusCell;
                model.statusFlags.isArrayBonusActive = arrayBonus.isArrayBonusActive;
                model.displayArrayBonusStatusText = ArrayBonusPlayerStatus(arrayBonus);
                model.lightingPreview.isOnArrayBonusCell = arrayBonus.isOnArrayBonusCell;
                model.lightingPreview.isArrayBonusActive = arrayBonus.isArrayBonusActive;
            }
            else
            {
                model.displayArrayBonusStatusText = query.ArrayBonusFallbackText;
            }

            if (buildItem != null)
            {
                model.statusFlags.countedInBuild = buildItem.countedInBuild;
                model.buildPreview.countedInBuild = buildItem.countedInBuild;
            }

            if (awakening != null)
            {
                model.statusFlags.coreEffectUnlocked = awakening.coreEffectUnlocked;
                model.statusFlags.coreEffectActive = awakening.coreEffectActive;
                model.statusFlags.inputLevel = awakening.inputLevel;
                model.statusFlags.resolvedLevel = awakening.resolvedLevel;
                model.statusFlags.itemLevel = awakening.itemLevel;
                model.statusFlags.unlockedCoreEffectCount = awakening.unlockedCoreEffectCount;
                model.statusFlags.activeCoreEffectCount = awakening.activeCoreEffectCount;
                model.statusFlags.currentAwakeningNodeLevel = awakening.currentUnlockedNodeLevel;
                model.statusFlags.nextAwakeningNodeLevel = awakening.nextUnlockLevel;
                model.displayAwakeningStatusText = AwakeningPlayerStatus(lighting, awakening);
                model.awakeningPreview.inputLevel = awakening.inputLevel;
                model.awakeningPreview.resolvedLevel = awakening.resolvedLevel;
                model.awakeningPreview.itemLevel = awakening.itemLevel;
                model.awakeningPreview.coreEffectUnlocked = awakening.coreEffectUnlocked;
                model.awakeningPreview.coreEffectActive = awakening.coreEffectActive;
                model.awakeningPreview.unlockedCoreEffectCount = awakening.unlockedCoreEffectCount;
                model.awakeningPreview.activeCoreEffectCount = awakening.activeCoreEffectCount;
                model.awakeningPreview.currentNodeText = FormatNodeText("当前开窍", awakening.currentUnlockedNodeLevel);
                model.awakeningPreview.nextNodeText = FormatNodeText("下一节点", awakening.nextUnlockLevel);
                model.awakeningPreview.unlockedNodeLevelsText = FormatLevels(awakening.UnlockedNodeLevels);
                model.awakeningPreview.activeNodeLevelsText = FormatLevels(awakening.ActiveNodeLevels);
                model.awakeningPreview.unlockedCoreEffectIdsText = FormatList(awakening.UnlockedCoreEffectIds);
                model.awakeningPreview.activeCoreEffectIdsText = FormatList(awakening.ActiveCoreEffectIds);
                model.awakeningPreview.lockedCoreEffectIdsText = FormatList(awakening.LockedCoreEffectIds);
                model.awakeningPreview.validationErrorsText = FormatList(awakening.ValidationErrors);
                model.awakeningPreview.readOnlyInputText = $"{awakening.inputSource}: inputLevel={awakening.inputLevel}, resolvedLevel={awakening.resolvedLevel}";
                model.battleEffectPreview.coreEffectActive = awakening.coreEffectActive;
            }
            else
            {
                model.displayAwakeningStatusText = query.AwakeningFallbackText;
                model.awakeningPreview.validationErrorsText = query.AwakeningFallbackText;
            }

            if (skillMonitor != null)
            {
                model.skillMonitorPreview.selectedMainBuildId = string.IsNullOrWhiteSpace(skillMonitor.selectedMainBuildId)
                    ? "None"
                    : skillMonitor.selectedMainBuildId;
                model.skillMonitorPreview.selectedMainBuildFound = skillMonitor.selectedMainBuildFound;
                model.skillMonitorPreview.selectedMainBuildCountText = skillMonitor.selectedMainBuildCount.ToString();
                model.skillMonitorPreview.selectedMainBuildStageText = ItemBuildTrackResult.BuildStageLabel(skillMonitor.selectedMainBuildStage);
                model.skillMonitorPreview.validationErrorsText = ItemSkillMonitorResolver.FormatValidationErrors(skillMonitor.ValidationErrors);
                model.skillMonitorPreview.previewText = ItemSkillMonitorResolver.FormatOverview(skillMonitor);
                model.skillMonitorPreview.slotPreviewLines = skillMonitor.Slots
                    .OrderBy(slot => slot.slotOrder)
                    .Select(slot => new ItemDetailTextLine(slot.displayName, SkillMonitorPlayerLine(slot), slot.isMonitoring ? "Monitoring" : "Locked"))
                    .ToList();
            }
            else
            {
                model.skillMonitorPreview.previewText = query.SkillMonitorFallbackText;
                model.skillMonitorPreview.validationErrorsText = query.SkillMonitorFallbackText;
                model.skillMonitorPreview.slotPreviewLines = new List<ItemDetailTextLine>();
            }
        }

        private static List<ItemDetailSectionViewModel> BuildPlayerSections(
            ItemDetailViewModel model,
            ProjectionQueryState query,
            ItemLightingItemResult lighting,
            ItemArrayBonusItemResult arrayBonus,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemBuildSynergyItemResult buildItem,
            ItemCoreAwakeningItemResult awakening,
            ItemSkillMonitorResolutionResult skillMonitor)
        {
            if (string.Equals(model.itemId, "I031", StringComparison.Ordinal))
            {
                return BuildSystemItemPlayerSections(model, query, lighting, arrayBonus, buildSnapshot, buildItem, awakening);
            }

            List<ItemDetailSectionViewModel> sections = new()
            {
                Section("物品强度", NonEmpty(model.displayItemPower, ItemDetailPresentationFormatter.ItemPowerUnavailable), "itemPower"),
                Section("基础属性", BuildCatalogStatsSection(model), "stats"),
                Section("触发条件", Indent(NonEmpty(model.displayTriggerText, UnconfiguredText)), "trigger"),
                Section("点亮与基础效果", BuildCurrentStateSection(query, lighting, arrayBonus, buildSnapshot, buildItem, awakening)
                    + "\n" + BuildBasicEffectSection(model, query, lighting), "basic"),
                Section("固定词条", BuildTextLinesOrReserved(model.displayFixedAffixes), "fixedAffix"),
                Section("随机词条", BuildTextLinesOrReserved(model.displayRandomAffixes), "randomAffix"),
                new ItemDetailSectionViewModel(string.Empty, "────────", "divider", true),
                IsOrange(model)
                    ? Section("道品成长痕迹", Indent(NonEmpty(model.displayOrangeAffix, "道品成长内容尚未配置")), "orange")
                    : new ItemDetailSectionViewModel(string.Empty, string.Empty, "orange", false),
                Section("核心效果/开窍", BuildCoreEffectSection(model, query, awakening), "awakening"),
                Section("法门Build", BuildTrackSection(query, buildSnapshot, buildItem, buildItem?.faMenBuildId, true, lighting), "famenBuild"),
                Section("器类Build", BuildTrackSection(query, buildSnapshot, buildItem, buildItem?.qiLeiBuildId, false, lighting), "qileiBuild"),
                Section("主Build自动监控", BuildSkillMonitorSection(query, skillMonitor), "skillMonitor"),
                new ItemDetailSectionViewModel(string.Empty, "────────", "divider", true),
                Section("摆放提示", BuildPlacementSection(model, query, lighting, arrayBonus), "placement"),
                Section("文化描述", Indent(NonEmpty(model.displayFlavorText, UnconfiguredText)), "flavor")
            };
            return sections;
        }

        private static List<ItemDetailSectionViewModel> BuildSystemItemPlayerSections(
            ItemDetailViewModel model,
            ProjectionQueryState query,
            ItemLightingItemResult lighting,
            ItemArrayBonusItemResult arrayBonus,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemBuildSynergyItemResult buildItem,
            ItemCoreAwakeningItemResult awakening)
        {
            return new List<ItemDetailSectionViewModel>
            {
                Section("物品强度", ItemDetailPresentationFormatter.ItemPowerUnavailable, "itemPower"),
                Section("基础属性", Indent("系统道具不参与普通实例属性生成"), "stats"),
                Section("触发条件", Indent(NonEmpty(model.displayTriggerText, "作为聚念与点亮源使用")), "trigger"),
                Section("点亮与基础效果", BuildCurrentStateSection(query, lighting, arrayBonus, buildSnapshot, buildItem, awakening)
                    + "\n" + BuildBasicEffectSection(model, query, lighting), "basic"),
                Section("固定词条", Indent("不适用：系统道具不进入普通词条生成"), "fixedAffix"),
                Section("随机词条", Indent("不适用：系统道具不进入普通词条生成"), "randomAffix"),
                new ItemDetailSectionViewModel(string.Empty, "────────", "divider", true),
                new ItemDetailSectionViewModel(string.Empty, string.Empty, "orange", false),
                Section("核心效果/开窍", Indent("不适用：系统道具没有普通核心候选与开窍节点"), "awakening"),
                Section("法门Build", Indent("不适用：系统道具不提供法门 Build 资格"), "famenBuild"),
                Section("器类Build", Indent("不适用：系统道具不提供器类 Build 资格"), "qileiBuild"),
                Section("主Build自动监控", Indent("不适用：不计入主 Build；技能载荷未执行"), "skillMonitor"),
                new ItemDetailSectionViewModel(string.Empty, "────────", "divider", true),
                Section("摆放提示", BuildPlacementSection(model, query, lighting, arrayBonus), "placement"),
                Section("文化描述", Indent(NonEmpty(model.displayFlavorText, UnconfiguredText)), "flavor")
            };
        }

        private static string BuildHeaderSection(ItemDetailViewModel model)
        {
            List<string> lines = new()
            {
                $"符器图占位：{NonEmpty(model.iconPlaceholderKey, "旧纸灰盒占位")}",
                $"名称：{NonEmpty(model.displayItemName, UnconfiguredText)}",
                $"品阶：{NonEmpty(model.displayRarityName, UnconfiguredText)}",
                $"稀有度色条：{NonEmpty(model.rarityColorKey, "default")}"
            };
            if (int.TryParse(model.displayItemPower, out int itemPower) && itemPower >= 0)
            {
                lines.Add($"物品强度：{itemPower}");
            }

            return IndentLines(lines);
        }

        private static string BuildIdentitySection(ItemDetailViewModel model)
        {
            return IndentLines(new[]
            {
                $"法门：{NonEmpty(model.displayFaMenName, UnconfiguredText)}",
                $"器类：{NonEmpty(model.displayQiLeiName, UnconfiguredText)}",
                $"形状：{SanitizeShapeName(model.displayShapeName)}",
                $"核心格：{CoreCellDisplayName(model.displayQiLeiName)}"
            });
        }

        private static string BuildCurrentStateSection(
            ProjectionQueryState query,
            ItemLightingItemResult lighting,
            ItemArrayBonusItemResult arrayBonus,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemBuildSynergyItemResult buildItem,
            ItemCoreAwakeningItemResult awakening)
        {
            return IndentLines(new[]
            {
                $"点亮状态：{(lighting == null ? query.LightingFallbackText : LightingPlayerStatus(lighting))}",
                $"阵脉状态：{(arrayBonus == null ? query.ArrayBonusFallbackText : ArrayBonusPlayerStatus(arrayBonus))}"
            });
        }

        private static string BuildCatalogStatsSection(ItemDetailViewModel model)
        {
            List<string> lines = FilterPlayerStats(model.displayPrimaryStats)
                .Select(stat => string.IsNullOrWhiteSpace(stat.hint)
                    ? $"{stat.label}：{stat.value}"
                    : $"{stat.label}：{stat.value}（{stat.hint}）")
                .ToList();
            return lines.Count == 0 ? Indent(UnconfiguredText) : IndentLines(lines);
        }

        private static string BuildBasicEffectSection(
            ItemDetailViewModel model,
            ProjectionQueryState query,
            ItemLightingItemResult lighting)
        {
            string state = lighting == null
                ? query.LightingFallbackText
                : lighting.isLit
                    ? "可生效"
                    : "未点亮，暂不生效";
            List<string> lines = new() { $"状态：{state}" };
            lines.AddRange(model.displayBasicEffects
                .Where(line => line != null && !string.IsNullOrWhiteSpace(line.body))
                .Select(line => string.IsNullOrWhiteSpace(line.title) ? line.body : $"{line.title}：{line.body}"));
            if (lines.Count == 1)
            {
                lines.Add(UnconfiguredText);
            }

            return IndentLines(lines);
        }

        private static string BuildCoreEffectSection(
            ItemDetailViewModel model,
            ProjectionQueryState query,
            ItemCoreAwakeningItemResult awakening)
        {
            if (awakening == null)
            {
                return Indent(query.AwakeningFallbackText);
            }

            if (!awakening.supportsAwakening || awakening.NodeStates.Count == 0)
            {
                return Indent(ReservedText);
            }

            List<ItemCoreAwakeningNodeState> visibleNodes = awakening.NodeStates
                .Where(node => !string.Equals(
                    node.blockedReason,
                    ItemCoreAwakeningBlockedReason.ReservedRarityGate.ToString(),
                    StringComparison.Ordinal))
                .OrderBy(node => node.unlockLevel)
                .ToList();
            int hiddenCount = Math.Max(0, awakening.NodeStates.Count - visibleNodes.Count);
            List<string> lines = new();
            foreach (ItemCoreAwakeningNodeState node in visibleNodes)
            {
                string status = node.isActive
                    ? "已开窍且可生效"
                    : node.isUnlocked
                        ? "已开窍，点亮后生效"
                        : "未开窍";
                string effect = NonEmpty(node.previewText, ReservedText);
                lines.Add($"Lv.{node.unlockLevel}：{status}；{effect}；{ItemDetailPresentationFormatter.EffectPayloadUnavailable}");
            }

            if (hiddenCount > 0)
            {
                lines.Add($"更高品阶可见候选：{hiddenCount} 项");
            }

            if (lines.Count == 0)
            {
                lines.Add("当前品阶没有可见核心效果");
            }

            return IndentLines(lines);
        }

        private static string BuildTrackSection(
            ProjectionQueryState query,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemBuildSynergyItemResult buildItem,
            string buildId,
            bool isFaMen,
            ItemLightingItemResult lighting)
        {
            if (buildSnapshot == null || (query.RequiresPlacedInstance && buildItem == null))
            {
                return Indent(query.BuildFallbackText);
            }

            ItemBuildTrackResult track = string.IsNullOrWhiteSpace(buildId)
                ? null
                : isFaMen
                    ? buildSnapshot.FindFaMenBuild(buildId)
                    : buildSnapshot.FindQiLeiBuild(buildId);
            if (track == null)
            {
                return Indent("当前道具没有可统计的Build轨道。");
            }

            List<string> lines = new()
            {
                $"当前数量：{track.litItemCount}/{track.maxPieceCount}",
                isFaMen
                    ? $"阶段：Build2 {(track.build2Active ? "已激活" : "未激活")} / Build4 {(track.build4Active ? "已激活" : "未激活")} / Build6 {(track.build6Active ? "已激活" : "未激活")}"
                    : $"阶段：Build2 {(track.build2Active ? "已激活" : "未激活")} / Build4 {(track.build4Active ? "已激活" : "未激活")}",
                $"下一阶段：{(track.nextStagePieceCount > 0 ? $"Build{track.nextStagePieceCount}" : "已达当前预览上限")}"
            };
            lines.Add("阶段效果：" + ItemDetailPresentationFormatter.EffectPayloadUnavailable);

            if (lighting != null && !lighting.isLit && !lighting.isLightingSource)
            {
                lines.Add("未点亮道具：不计入当前Build。");
            }

            return IndentLines(lines);
        }

        private static string BuildSkillMonitorSection(
            ProjectionQueryState query,
            ItemSkillMonitorResolutionResult skillMonitor)
        {
            if (skillMonitor == null)
            {
                return Indent(query.SkillMonitorFallbackText);
            }

            List<string> lines = new()
            {
                $"主Build：{(string.IsNullOrWhiteSpace(skillMonitor.selectedMainBuildId) ? "尚未选择" : "已选择")}",
                "技能载荷尚未配置；本页未执行任何技能触发"
            };
            foreach (ItemSkillMonitorSlotSnapshot slot in skillMonitor.Slots.OrderBy(slot => slot.slotOrder))
            {
                lines.Add($"{slot.displayName}：{(slot.isMonitoring ? "监控中" : "未解锁")}");
            }

            return IndentLines(lines);
        }

        private static string BuildPlacementSection(
            ItemDetailViewModel model,
            ProjectionQueryState query,
            ItemLightingItemResult lighting,
            ItemArrayBonusItemResult arrayBonus)
        {
            List<string> lines = new()
            {
                $"形状：{SanitizeShapeName(model.displayShapeName)}",
                $"核心格类型：{CoreCellDisplayName(model.displayQiLeiName)}",
                $"点亮提示：{(lighting == null ? query.LightingFallbackText : LightingPlayerStatus(lighting))}",
                $"阵脉提示：{(arrayBonus == null ? query.ArrayBonusFallbackText : ArrayBonusPlayerStatus(arrayBonus))}"
            };
            lines.AddRange(model.displayPlacementTips
                .Where(line => line != null && !string.IsNullOrWhiteSpace(line.body))
                .Select(line => line.body));
            return IndentLines(lines);
        }

        private static List<ItemDetailSectionViewModel> BuildDebugSections(
            ItemDetailViewModel model,
            ProjectionQueryState query,
            ItemLightingItemResult lighting,
            ItemArrayBonusItemResult arrayBonus,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemBuildSynergyItemResult buildItem,
            ItemCoreAwakeningItemResult awakening,
            ItemSkillMonitorResolutionResult skillMonitor)
        {
            List<string> identity = new()
            {
                $"itemId: {FormatNullable(model.itemId)}",
                $"placementId: {FormatNullable(model.placementId)}",
                $"contextKind: {query.contextKind}",
                $"requestedPlacementId: {FormatNullable(query.requestedPlacementId)}",
                $"shapeCells: {FindStatValue(model.displayPrimaryStats, "shapeCells")}",
                $"occupiedCells: {(lighting == null ? query.LightingFallbackText : FormatCells(lighting.OccupiedCells))}",
                $"coreCellLocal: {FindStatValue(model.displayPrimaryStats, "coreCellLocal")}",
                $"coreCellWorld: {(lighting == null ? query.LightingFallbackText : FormatCell(lighting.coreCellWorld))}"
            };

            List<string> state = new()
            {
                $"isDirectLit: {DebugBool(lighting?.isDirectLit, lighting != null)}",
                $"isLit: {DebugBool(lighting?.isLit, lighting != null)}",
                $"litByItemId: {(lighting == null ? query.LightingFallbackText : FormatNullable(lighting.litByItemId))}",
                $"litByPlacementId: {(lighting == null ? query.LightingFallbackText : FormatNullable(lighting.litByPlacementId))}",
                $"litDepth: {(lighting == null ? query.LightingFallbackText : lighting.litDepth >= 0 ? lighting.litDepth.ToString() : "None")}",
                $"occupiedArrayBonusCells: {(arrayBonus == null ? query.ArrayBonusFallbackText : FormatCells(arrayBonus.OccupiedArrayBonusCells))}",
                $"isOnArrayBonusCell: {DebugBool(arrayBonus?.isOnArrayBonusCell, arrayBonus != null)}",
                $"isArrayBonusActive: {DebugBool(arrayBonus?.isArrayBonusActive, arrayBonus != null)}",
                $"countedInBuild: {DebugBool(buildItem?.countedInBuild, buildItem != null)}",
                $"inputLevel: {(awakening == null ? query.AwakeningFallbackText : awakening.inputLevel.ToString())}",
                $"resolvedLevel: {(awakening == null ? query.AwakeningFallbackText : awakening.resolvedLevel.ToString())}",
                $"unlockedCoreEffectIds: {(awakening == null ? query.AwakeningFallbackText : FormatList(awakening.UnlockedCoreEffectIds))}",
                $"activeCoreEffectIds: {(awakening == null ? query.AwakeningFallbackText : FormatList(awakening.ActiveCoreEffectIds))}",
                $"selectedMainBuildId: {(skillMonitor == null ? query.SkillMonitorFallbackText : FormatNullable(skillMonitor.selectedMainBuildId))}"
            };
            if (string.Equals(model.itemId, "I031", StringComparison.Ordinal))
            {
                state.Add("ordinaryGenerationEligible: false");
                state.Add("ordinaryAffixEligible: false");
                state.Add("buildQualification: None");
                state.Add("corePotentialEligible: false");
                state.Add("systemItemBoundary: LIGHTING_SOURCE_ONLY");
            }

            if (skillMonitor == null)
            {
                state.Add("BasicAttack: " + query.SkillMonitorFallbackText);
                state.Add("Build2: " + query.SkillMonitorFallbackText);
                state.Add("Build4: " + query.SkillMonitorFallbackText);
                state.Add("Build6: " + query.SkillMonitorFallbackText);
            }
            else
            {
                foreach (ItemSkillMonitorSlotSnapshot slot in skillMonitor.Slots.OrderBy(slot => slot.slotOrder))
                {
                    state.Add($"{slot.slotType}: {(slot.isMonitoring ? "Monitoring" : "Locked")}");
                }
            }

            List<string> errors = new();
            errors.Add($"projectionContextKind: {query.contextKind}");
            errors.Add($"projectionPlacementId: {FormatNullable(query.requestedPlacementId)}");
            foreach (string missing in query.MissingSnapshotDebugLines)
            {
                errors.Add(missing);
            }

            AddSnapshotErrors(errors, "build", buildSnapshot?.ValidationErrors);
            AddSnapshotErrors(errors, "awakening", awakening?.ValidationErrors);
            AddSnapshotErrors(errors, "skillMonitor", skillMonitor?.ValidationErrors);
            if (errors.Count == 2)
            {
                errors.Add("validationErrors: None");
            }

            return new List<ItemDetailSectionViewModel>
            {
                Section("调试 / 身份与坐标", IndentLines(identity), "debugIdentity"),
                Section("调试 / 状态快照", IndentLines(state), "debugState"),
                Section("调试 / validationErrors", IndentLines(errors), "debugErrors")
            };
        }

        private static string LightingPlayerStatus(ItemLightingItemResult lighting)
        {
            if (lighting == null)
            {
                return MissingState;
            }

            if (lighting.isLightingSource)
            {
                return "点亮源";
            }

            if (lighting.isDirectLit)
            {
                return "直接点亮";
            }

            if (lighting.IsRelayLit)
            {
                return "相邻接亮";
            }

            return "未点亮";
        }

        private static string RelayPlayerStatus(ItemLightingItemResult lighting)
        {
            if (lighting == null)
            {
                return MissingState;
            }

            if (lighting.IsRelayLit)
            {
                return "相邻接亮";
            }

            if (lighting.isLit)
            {
                return "已点亮";
            }

            return "未点亮";
        }

        private static string ArrayBonusPlayerStatus(ItemArrayBonusItemResult arrayBonus)
        {
            if (arrayBonus == null)
            {
                return MissingState;
            }

            if (arrayBonus.isArrayBonusActive)
            {
                return "阵脉生效";
            }

            if (arrayBonus.isOnArrayBonusCell)
            {
                return "阵脉待亮";
            }

            return "未占阵脉";
        }

        private static string AwakeningPlayerStatus(ItemLightingItemResult lighting, ItemCoreAwakeningItemResult awakening)
        {
            if (awakening == null)
            {
                return MissingState;
            }

            if (awakening.coreEffectActive)
            {
                return "已开窍且可生效";
            }

            if (awakening.coreEffectUnlocked)
            {
                return "已开窍，点亮后生效";
            }

            return "尚未开窍";
        }

        private static string BuildCountStatus(
            ProjectionQueryState query,
            ItemBuildSynergyResolutionResult buildSnapshot,
            ItemBuildSynergyItemResult buildItem)
        {
            if (buildSnapshot == null || (query.RequiresPlacedInstance && buildItem == null))
            {
                return query.BuildFallbackText;
            }

            return buildItem?.countedInBuild == true ? "已计入Build" : "当前不计入Build";
        }

        private static string SkillMonitorPlayerLine(ItemSkillMonitorSlotSnapshot slot)
        {
            return slot.isMonitoring ? "Monitoring" : "Locked";
        }

        private static IEnumerable<ItemDetailStatLine> FilterPlayerStats(IReadOnlyList<ItemDetailStatLine> stats)
        {
            return (stats ?? Array.Empty<ItemDetailStatLine>())
                .Where(stat => stat != null
                    && !string.IsNullOrWhiteSpace(stat.value)
                    && !IsRawOrInternalLabel(stat.label));
        }

        private static bool IsRawOrInternalLabel(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return true;
            }

            string value = label.Trim();
            string[] rawLabels =
            {
                "ItemId",
                "itemId",
                "itemFamily",
                "faMenTag",
                "qiLeiTag",
                "shapeCells",
                "occupiedCells",
                "coreCellLocal",
                "coreCellWorld",
                "placementId",
                "allowedRarities",
                "lightingSource",
                "validationErrors",
                "coreEffectId",
                "selectedMainBuildId",
                "isLit",
                "isDirectLit",
                "litDepth"
            };
            return rawLabels.Any(raw => string.Equals(raw, value, StringComparison.OrdinalIgnoreCase));
        }

        private static ProjectionQueryState ResolveQueryState(ItemDetailProjectionInput input, string itemId)
        {
            ItemDetailProjectionContextKind contextKind = input.contextKind;
            string placementId = string.IsNullOrWhiteSpace(input.placementId)
                ? string.Empty
                : input.placementId.Trim();
            return new ProjectionQueryState(contextKind, itemId, placementId);
        }

        private static ItemLightingItemResult FindLighting(ItemLightingResolutionResult snapshot, string placementId)
        {
            if (snapshot == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(placementId))
            {
                ItemLightingItemResult result = snapshot.FindPlacementResult(placementId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static ItemArrayBonusItemResult FindArrayBonus(ItemArrayBonusResolutionResult snapshot, string placementId)
        {
            if (snapshot == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(placementId))
            {
                ItemArrayBonusItemResult result = snapshot.FindPlacementResult(placementId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static ItemBuildSynergyItemResult FindBuildItem(ItemBuildSynergyResolutionResult snapshot, string placementId)
        {
            if (snapshot == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(placementId))
            {
                ItemBuildSynergyItemResult result = snapshot.FindPlacementResult(placementId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static ItemCoreAwakeningItemResult FindAwakening(ItemCoreAwakeningResolutionResult snapshot, string placementId)
        {
            if (snapshot == null)
            {
                return null;
            }

            if (!string.IsNullOrWhiteSpace(placementId))
            {
                ItemCoreAwakeningItemResult result = snapshot.FindPlacementResult(placementId);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static string BuildTextLinesOrReserved(IReadOnlyList<ItemDetailTextLine> lines)
        {
            List<string> values = (lines ?? Array.Empty<ItemDetailTextLine>())
                .Where(line => line != null && !string.IsNullOrWhiteSpace(line.body))
                .Select(line => string.IsNullOrWhiteSpace(line.title) ? line.body : $"{line.title}：{line.body}")
                .ToList();
            return values.Count == 0 ? Indent(ReservedText) : IndentLines(values);
        }

        private static ItemDetailSectionViewModel Section(string title, string body, string stateKey)
        {
            return new ItemDetailSectionViewModel(title, body, stateKey, true);
        }

        private static string Indent(string text)
        {
            return "  " + NonEmpty(text, string.Empty).Replace("\n", "\n  ");
        }

        private static string IndentLines(IEnumerable<string> lines)
        {
            StringBuilder builder = new();
            foreach (string line in lines ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                builder.Append("  ").Append(line);
            }

            return builder.Length == 0 ? Indent(UnconfiguredText) : builder.ToString();
        }

        private static string NonEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static bool IsOrange(ItemDetailViewModel model)
        {
            string key = string.IsNullOrWhiteSpace(model?.rarityKey)
                ? model?.displayRarityName
                : model.rarityKey;
            return !string.IsNullOrWhiteSpace(key)
                && (key.Contains("orange", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("道", StringComparison.Ordinal)
                    || key.Contains("橙", StringComparison.Ordinal));
        }

        private static string NormalizePlacementId(string first, string second)
        {
            if (!string.IsNullOrWhiteSpace(first))
            {
                return first.Trim();
            }

            return string.IsNullOrWhiteSpace(second) ? string.Empty : second.Trim();
        }

        private static string SanitizeShapeName(string shapeName)
        {
            if (string.IsNullOrWhiteSpace(shapeName))
            {
                return UnconfiguredText;
            }

            string value = shapeName.Trim();
            if (ContainsOrdinal(value, "square_4"))
            {
                return "方块四格";
            }

            if (ContainsOrdinal(value, "corner_3"))
            {
                return "折角三格";
            }

            if (ContainsOrdinal(value, "vertical_3"))
            {
                return "竖排三格";
            }

            if (ContainsOrdinal(value, "vertical_2"))
            {
                return "竖排两格";
            }

            if (ContainsOrdinal(value, "single_1"))
            {
                return "单格";
            }

            return value;
        }

        private static string CoreCellDisplayName(string qiLeiName)
        {
            if (string.IsNullOrWhiteSpace(qiLeiName))
            {
                return "核心格";
            }

            if (ContainsOrdinal(qiLeiName, "符"))
            {
                return "符胆格";
            }

            if (ContainsOrdinal(qiLeiName, "印"))
            {
                return "印心格";
            }

            if (ContainsOrdinal(qiLeiName, "令"))
            {
                return "令心格";
            }

            if (ContainsOrdinal(qiLeiName, "镜"))
            {
                return "镜心格";
            }

            if (ContainsOrdinal(qiLeiName, "法"))
            {
                return "器心格";
            }

            return "中宫格";
        }

        private static string FindStatValue(IReadOnlyList<ItemDetailStatLine> stats, string label)
        {
            ItemDetailStatLine stat = (stats ?? Array.Empty<ItemDetailStatLine>())
                .FirstOrDefault(line => line != null && string.Equals(line.label, label, StringComparison.OrdinalIgnoreCase));
            return stat == null || string.IsNullOrWhiteSpace(stat.value) ? "None" : stat.value;
        }

        private static string FormatNodeText(string prefix, int level)
        {
            return level > 0 ? $"{prefix}: Lv.{level}" : $"{prefix}: None";
        }

        private static string FormatLevels(IReadOnlyList<int> levels)
        {
            return levels == null || levels.Count == 0
                ? "None"
                : string.Join("|", levels.Select(level => $"Lv.{level}"));
        }

        private static string FormatList(IReadOnlyList<string> values)
        {
            return values == null || values.Count == 0 ? "None" : string.Join("|", values);
        }

        private static string FormatNullable(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "None" : value;
        }

        private static string FormatCells(IReadOnlyList<Vector2Int> cells)
        {
            return cells == null || cells.Count == 0
                ? "None"
                : string.Join(";", cells.Select(FormatCell));
        }

        private static string FormatCell(Vector2Int cell)
        {
            return $"({cell.x},{cell.y})";
        }

        private static string DebugBool(bool? value, bool hasSnapshot)
        {
            return hasSnapshot ? (value == true ? "true" : "false") : MissingState;
        }

        private static void AddSnapshotErrors(List<string> errors, string source, IReadOnlyList<string> values)
        {
            if (values == null || values.Count == 0)
            {
                return;
            }

            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"{source}: {value}");
                }
            }
        }

        private sealed class ProjectionQueryState
        {
            private readonly List<string> missingSnapshotDebugLines = new();

            public ProjectionQueryState(ItemDetailProjectionContextKind contextKind, string itemId, string requestedPlacementId)
            {
                this.contextKind = contextKind;
                this.itemId = itemId ?? string.Empty;
                this.requestedPlacementId = string.IsNullOrWhiteSpace(requestedPlacementId)
                    ? string.Empty
                    : requestedPlacementId.Trim();
                resolvedPlacementId = contextKind == ItemDetailProjectionContextKind.PlacedInstance
                    ? this.requestedPlacementId
                    : string.Empty;
            }

            public ItemDetailProjectionContextKind contextKind { get; }
            public string itemId { get; }
            public string requestedPlacementId { get; }
            public string resolvedPlacementId { get; private set; }
            public IReadOnlyList<string> MissingSnapshotDebugLines => missingSnapshotDebugLines;

            public bool RequiresPlacedInstance => contextKind == ItemDetailProjectionContextKind.PlacedInstance;

            public bool CanReadPlacementState => RequiresPlacedInstance
                && !string.IsNullOrWhiteSpace(requestedPlacementId);

            public string LightingFallbackText { get; private set; } = MissingState;
            public string ArrayBonusFallbackText { get; private set; } = MissingState;
            public string BuildFallbackText { get; private set; } = MissingState;
            public string AwakeningFallbackText { get; private set; } = MissingState;
            public string SkillMonitorFallbackText { get; private set; } = MissingState;

            public void FinalizeState(
                ItemDetailProjectionInput input,
                ItemLightingItemResult lighting,
                ItemArrayBonusItemResult arrayBonus,
                ItemBuildSynergyItemResult buildItem,
                ItemCoreAwakeningItemResult awakening,
                ItemSkillMonitorResolutionResult skillMonitor)
            {
                resolvedPlacementId = RequiresPlacedInstance ? requestedPlacementId : string.Empty;
                LightingFallbackText = BuildPlacementFallback("Lighting", input.lightingSnapshot != null, lighting != null);
                ArrayBonusFallbackText = BuildPlacementFallback("ArrayBonus", input.arrayBonusSnapshot != null, arrayBonus != null);
                BuildFallbackText = BuildPlacementFallback("Build", input.buildSnapshot != null, buildItem != null);
                AwakeningFallbackText = BuildPlacementFallback("CoreAwakening", input.coreAwakeningSnapshot != null, awakening != null);
                SkillMonitorFallbackText = BuildSkillMonitorFallback(input.skillMonitorSnapshot != null, skillMonitor != null);
            }

            private string BuildPlacementFallback(string snapshotType, bool hasSnapshot, bool hasPlacementResult)
            {
                if (contextKind == ItemDetailProjectionContextKind.CatalogPreview)
                {
                    missingSnapshotDebugLines.Add(MissingLine(snapshotType, "ignored-by-catalog-preview"));
                    return StateFallbackFor(snapshotType, "catalog");
                }

                if (string.IsNullOrWhiteSpace(requestedPlacementId))
                {
                    missingSnapshotDebugLines.Add(MissingLine(snapshotType, "missing-placementId"));
                    return StateFallbackFor(snapshotType, "missingPlacement");
                }

                if (!hasSnapshot)
                {
                    missingSnapshotDebugLines.Add(MissingLine(snapshotType, "snapshot-missing"));
                    return MissingState;
                }

                if (!hasPlacementResult)
                {
                    missingSnapshotDebugLines.Add(MissingLine(snapshotType, "placement-not-found"));
                    return InstanceUnavailableState;
                }

                return MissingState;
            }

            private static string StateFallbackFor(string snapshotType, string reason)
            {
                if (string.Equals(snapshotType, "Lighting", StringComparison.Ordinal))
                {
                    return "尚未入阵";
                }

                if (string.Equals(snapshotType, "ArrayBonus", StringComparison.Ordinal))
                {
                    return "阵脉未计算";
                }

                return string.Equals(reason, "catalog", StringComparison.Ordinal)
                    ? CatalogPreviewState
                    : InstanceUnavailableState;
            }

            private string BuildSkillMonitorFallback(bool hasSnapshot, bool hasMonitor)
            {
                if (contextKind == ItemDetailProjectionContextKind.CatalogPreview)
                {
                    missingSnapshotDebugLines.Add(MissingLine("SkillMonitor", "ignored-by-catalog-preview"));
                    return CatalogPreviewState;
                }

                if (string.IsNullOrWhiteSpace(requestedPlacementId))
                {
                    missingSnapshotDebugLines.Add(MissingLine("SkillMonitor", "missing-placementId"));
                    return InstanceUnavailableState;
                }

                if (!hasSnapshot || !hasMonitor)
                {
                    missingSnapshotDebugLines.Add(MissingLine("SkillMonitor", "snapshot-missing"));
                    return MissingState;
                }

                return MissingState;
            }

            private string MissingLine(string snapshotType, string reason)
            {
                return $"missingSnapshot: {snapshotType} placementId={FormatNullable(requestedPlacementId)} itemId={FormatNullable(itemId)} reason={reason}";
            }
        }

        private static bool ContainsOrdinal(string value, string fragment)
        {
            return !string.IsNullOrEmpty(value)
                && !string.IsNullOrEmpty(fragment)
                && value.IndexOf(fragment, StringComparison.Ordinal) >= 0;
        }
    }
}
