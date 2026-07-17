using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Awakening;
using TalismanBag.Items.Build;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Lighting;

namespace TalismanBag.ItemSandbox
{
    public static class ItemSandboxLightingDetailProjection
    {
        public static ItemDetailViewModel Project(
            ItemDetailViewModel source,
            ItemLightingItemResult lightingResult,
            ItemArrayBonusItemResult arrayBonusResult = null,
            ItemBuildSynergyResolutionResult buildResult = null,
            ItemCoreAwakeningItemResult awakeningResult = null)
        {
            ItemDetailViewModel model = Clone(source);
            if (model == null || lightingResult == null)
            {
                return model;
            }

            bool basicEffectActive = lightingResult.isLit && !lightingResult.isLightingSource;
            bool isOnArrayBonusCell = arrayBonusResult != null && arrayBonusResult.isOnArrayBonusCell;
            bool isArrayBonusActive = arrayBonusResult != null && arrayBonusResult.isArrayBonusActive;
            string placementId = string.IsNullOrEmpty(lightingResult.placementId)
                ? lightingResult.itemId
                : lightingResult.placementId;

            model.placementId = placementId;
            model.statusFlags.placementId = placementId;
            model.statusFlags.isLit = lightingResult.isLit;
            model.statusFlags.isLightingSource = lightingResult.isLightingSource;
            model.statusFlags.isDirectLit = lightingResult.isDirectLit;
            model.statusFlags.isRelayLit = lightingResult.IsRelayLit;
            model.statusFlags.countedInBuild = false;
            model.statusFlags.litByItemId = lightingResult.litByItemId;
            model.statusFlags.litDepth = lightingResult.litDepth;
            model.statusFlags.basicEffectActive = basicEffectActive;
            model.statusFlags.isOnArrayBonusCell = isOnArrayBonusCell;
            model.statusFlags.isArrayBonusActive = isArrayBonusActive;
            model.displayLightingStatusText = BuildLightingText(lightingResult);
            model.displayArrayBonusStatusText = BuildArrayBonusText(arrayBonusResult);

            model.displayPrimaryStats.Add(new ItemDetailStatLine("placementId", placementId, "Read-only placed instance identity for same-item layouts."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("isDirectLit", lightingResult.isDirectLit ? "true" : "false", "Read-only lighting projection."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("isLit", lightingResult.isLit ? "true" : "false", "Read-only lighting projection."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("litByItemId", string.IsNullOrEmpty(lightingResult.litByItemId) ? "None" : lightingResult.litByItemId, "Source item id at minimum lighting depth."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("litByPlacementId", string.IsNullOrEmpty(lightingResult.litByPlacementId) ? "None" : lightingResult.litByPlacementId, "Source placed-instance id at minimum lighting depth."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("litDepth", lightingResult.litDepth >= 0 ? lightingResult.litDepth.ToString() : "None", "Direct lit depth is 0; relay starts at 1."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("basicEffectReady", basicEffectActive ? "true" : "false", "State only; no battle settlement is executed here."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("occupiedArrayBonusCells", FormatArrayBonusCells(arrayBonusResult), "Array bonus cells occupied by this placed instance."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("isOnArrayBonusCell", isOnArrayBonusCell ? "true" : "false", "occupiedCells overlaps AP01/AP02/AP03/AP04."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("isArrayBonusActive", isArrayBonusActive ? "true" : "false", "isLit && isOnArrayBonusCell; no numeric bonus is settled."));

            model.lightingPreview.placementId = placementId;
            model.lightingPreview.isLit = lightingResult.isLit;
            model.lightingPreview.isLightingSource = lightingResult.isLightingSource;
            model.lightingPreview.isDirectLit = lightingResult.isDirectLit;
            model.lightingPreview.isRelayLit = lightingResult.IsRelayLit;
            model.lightingPreview.isOnArrayBonusCell = isOnArrayBonusCell;
            model.lightingPreview.isArrayBonusActive = isArrayBonusActive;
            model.lightingPreview.litByItemId = lightingResult.litByItemId;
            model.lightingPreview.litDepth = lightingResult.litDepth;
            model.lightingPreview.basicEffectActive = basicEffectActive;
            model.lightingPreview.previewText = model.displayLightingStatusText;

            ApplyBuildProjection(model, lightingResult, buildResult);
            ApplyAwakeningProjection(model, lightingResult, awakeningResult, basicEffectActive);

            return model;
        }

        private static void ApplyAwakeningProjection(
            ItemDetailViewModel model,
            ItemLightingItemResult lightingResult,
            ItemCoreAwakeningItemResult awakeningResult,
            bool basicEffectActive)
        {
            if (model == null)
            {
                return;
            }

            if (awakeningResult == null && lightingResult != null)
            {
                awakeningResult = ItemCoreAwakeningResolver.ResolveItem(
                    lightingResult,
                    new ItemCoreAwakeningInput(lightingResult.itemId, lightingResult.placementId, 1, false, "MissingInputDefaultLv1"));
            }

            bool coreEffectUnlocked = awakeningResult?.coreEffectUnlocked == true;
            bool coreEffectActive = awakeningResult?.coreEffectActive == true;
            model.statusFlags.coreEffectUnlocked = coreEffectUnlocked;
            model.statusFlags.coreEffectActive = coreEffectActive;
            model.statusFlags.inputLevel = awakeningResult?.inputLevel ?? 1;
            model.statusFlags.resolvedLevel = awakeningResult?.resolvedLevel ?? 1;
            model.statusFlags.itemLevel = awakeningResult?.resolvedLevel ?? 1;
            model.statusFlags.unlockedCoreEffectCount = awakeningResult?.unlockedCoreEffectCount ?? 0;
            model.statusFlags.activeCoreEffectCount = awakeningResult?.activeCoreEffectCount ?? 0;
            model.statusFlags.currentAwakeningNodeLevel = awakeningResult?.currentUnlockedNodeLevel ?? 0;
            model.statusFlags.nextAwakeningNodeLevel = awakeningResult?.nextUnlockLevel ?? 0;

            model.displayAwakeningStatusText = ItemCoreAwakeningResolver.BuildStatusText(awakeningResult);
            model.displayPrimaryStats.Add(new ItemDetailStatLine("inputLevel", model.statusFlags.inputLevel.ToString(), "Raw read-only sandbox preview level; not saved."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("resolvedLevel", model.statusFlags.resolvedLevel.ToString(), "Clamp(inputLevel, 1, 40); not saved."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("coreEffectUnlocked", coreEffectUnlocked ? "true" : "false", "Unlocked by Lv.10 / 20 / 30 / 40 preview nodes only."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("coreEffectActive", coreEffectActive ? "true" : "false", "isLit && coreEffectUnlocked; no battle effect is executed."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("unlockedCoreNodes", ItemCoreAwakeningResolver.FormatNodeLevels(awakeningResult?.UnlockedNodeLevels), "Unlocked core-effect nodes."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("activeCoreNodes", ItemCoreAwakeningResolver.FormatNodeLevels(awakeningResult?.ActiveNodeLevels), "Currently fireable core-effect nodes."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("unlockedCoreEffectIds", ItemCoreAwakeningResolver.FormatIds(awakeningResult?.UnlockedCoreEffectIds), "Unlocked item-specific core effect ids."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("activeCoreEffectIds", ItemCoreAwakeningResolver.FormatIds(awakeningResult?.ActiveCoreEffectIds), "Currently fireable item-specific core effect ids."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("lockedCoreEffectIds", ItemCoreAwakeningResolver.FormatIds(awakeningResult?.LockedCoreEffectIds), "Locked item-specific core effect ids."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("awakeningValidationErrors", FormatIds(awakeningResult?.ValidationErrors), "Read-only resolver validation output."));

            model.awakeningPreview.inputLevel = model.statusFlags.inputLevel;
            model.awakeningPreview.resolvedLevel = model.statusFlags.resolvedLevel;
            model.awakeningPreview.itemLevel = model.statusFlags.itemLevel;
            model.awakeningPreview.coreEffectUnlocked = coreEffectUnlocked;
            model.awakeningPreview.coreEffectActive = coreEffectActive;
            model.awakeningPreview.unlockedCoreEffectCount = model.statusFlags.unlockedCoreEffectCount;
            model.awakeningPreview.activeCoreEffectCount = model.statusFlags.activeCoreEffectCount;
            model.awakeningPreview.currentNodeText = model.statusFlags.currentAwakeningNodeLevel > 0
                ? $"Current unlocked node: Lv.{model.statusFlags.currentAwakeningNodeLevel}"
                : "Current unlocked node: None";
            model.awakeningPreview.nextNodeText = model.statusFlags.nextAwakeningNodeLevel > 0
                ? $"Next node: Lv.{model.statusFlags.nextAwakeningNodeLevel}"
                : "Next node: max preview node reached";
            model.awakeningPreview.unlockedNodeLevelsText = ItemCoreAwakeningResolver.FormatNodeLevels(awakeningResult?.UnlockedNodeLevels);
            model.awakeningPreview.activeNodeLevelsText = ItemCoreAwakeningResolver.FormatNodeLevels(awakeningResult?.ActiveNodeLevels);
            model.awakeningPreview.unlockedCoreEffectIdsText = ItemCoreAwakeningResolver.FormatIds(awakeningResult?.UnlockedCoreEffectIds);
            model.awakeningPreview.activeCoreEffectIdsText = ItemCoreAwakeningResolver.FormatIds(awakeningResult?.ActiveCoreEffectIds);
            model.awakeningPreview.lockedCoreEffectIdsText = ItemCoreAwakeningResolver.FormatIds(awakeningResult?.LockedCoreEffectIds);
            model.awakeningPreview.validationErrorsText = FormatIds(awakeningResult?.ValidationErrors);
            model.awakeningPreview.readOnlyInputText = awakeningResult == null
                ? "No read-only awakening input."
                : $"{awakeningResult.inputSource}: inputLevel={awakeningResult.inputLevel}, resolvedLevel={awakeningResult.resolvedLevel}, highRarityUltimatePreview={awakeningResult.highRarityUltimatePreview}, requiredRarityKey={awakeningResult.requiredRarityKey}";

            model.displayCoreEffects.Clear();
            if (awakeningResult != null)
            {
                foreach (ItemCoreAwakeningNodeState node in awakeningResult.NodeStates)
                {
                    string body = $"coreEffectId={FormatNullable(node.coreEffectId)} / itemId={node.itemId} / nodeKind={node.nodeKind} / unlockLevel={node.unlockLevel} / unlocked={FormatBool(node.isUnlocked)} / active={FormatBool(node.isActive)} / stateKey={node.stateKey} / blockedReason={node.blockedReason} / {node.previewText}";
                    model.displayCoreEffects.Add(new ItemDetailTextLine(node.displayName, body, node.stateKey));
                }
            }

            if (model.displayCoreEffects.Count == 0)
            {
                model.displayCoreEffects.Add(new ItemDetailTextLine(
                    "核心效果",
                    "No core-effect node preview is available.",
                    "empty"));
            }

            model.battleEffectPreview.basicEffectActive = basicEffectActive;
            model.battleEffectPreview.coreEffectActive = coreEffectActive;
            model.battleEffectPreview.battleStateText = BuildBattleStateText(basicEffectActive, coreEffectUnlocked, coreEffectActive);
        }

        private static void ApplyBuildProjection(
            ItemDetailViewModel model,
            ItemLightingItemResult lightingResult,
            ItemBuildSynergyResolutionResult buildResult)
        {
            if (model == null)
            {
                return;
            }

            ItemBuildSynergyItemResult itemBuild = null;
            if (buildResult != null && lightingResult != null)
            {
                itemBuild = buildResult.FindPlacementResult(lightingResult.placementId)
                    ?? buildResult.FindItemResult(lightingResult.itemId);
            }

            model.statusFlags.countedInBuild = itemBuild?.countedInBuild == true;
            model.buildPreview.countedInBuild = model.statusFlags.countedInBuild;
            model.displayFaMenBuilds.Clear();
            model.displayQiLeiBuilds.Clear();

            if (itemBuild == null || buildResult == null)
            {
                model.buildPreview.faMenPreviewText = "Build snapshot unavailable; no Build count or activation is projected.";
                model.buildPreview.qiLeiPreviewText = "Build snapshot unavailable; no Build count or activation is projected.";
                return;
            }

            ItemBuildTrackResult faMenTrack = string.IsNullOrWhiteSpace(itemBuild.faMenBuildId)
                ? null
                : buildResult.FindFaMenBuild(itemBuild.faMenBuildId);
            ItemBuildTrackResult qiLeiTrack = string.IsNullOrWhiteSpace(itemBuild.qiLeiBuildId)
                ? null
                : buildResult.FindQiLeiBuild(itemBuild.qiLeiBuildId);
            string countedText = BuildCountedText(itemBuild);

            if (faMenTrack != null)
            {
                model.displayFaMenBuilds.Add(BuildTrackPreview(faMenTrack, countedText));
                model.buildPreview.faMenPreviewText = $"{faMenTrack.displayName}: {faMenTrack.progressText}. {countedText}";
            }
            else
            {
                model.buildPreview.faMenPreviewText = $"No valid faMen Build tag. {countedText}";
            }

            if (qiLeiTrack != null)
            {
                model.displayQiLeiBuilds.Add(BuildTrackPreview(qiLeiTrack, countedText));
                model.buildPreview.qiLeiPreviewText = $"{qiLeiTrack.displayName}: {qiLeiTrack.progressText}. {countedText}";
            }
            else
            {
                model.buildPreview.qiLeiPreviewText = $"No valid qiLei Build tag. {countedText}";
            }

            model.displayPrimaryStats.Add(new ItemDetailStatLine("countedInBuild", itemBuild.countedInBuild ? "true" : "false", "Only lit, non-JuNian items with valid faMenTag and qiLeiTag count."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("faMenBuildId", string.IsNullOrEmpty(itemBuild.faMenBuildId) ? "None" : itemBuild.faMenBuildId, "Read-only Build identity."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("faMenBuildCount", itemBuild.faMenBuildCount.ToString(), "Lit non-JuNian item count for this faMen tag."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("faMenActiveStage", ItemBuildTrackResult.BuildStageLabel(itemBuild.faMenActiveStagePieceCount), "FaMen stages: Build2 / Build4 / Build6."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("qiLeiBuildId", string.IsNullOrEmpty(itemBuild.qiLeiBuildId) ? "None" : itemBuild.qiLeiBuildId, "Read-only Build identity."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("qiLeiBuildCount", itemBuild.qiLeiBuildCount.ToString(), "Lit non-JuNian item count for this qiLei tag."));
            model.displayPrimaryStats.Add(new ItemDetailStatLine("qiLeiActiveStage", ItemBuildTrackResult.BuildStageLabel(itemBuild.qiLeiActiveStagePieceCount), "QiLei stages: Build2 / Build4."));

            if (buildResult.ValidationErrors.Count > 0)
            {
                model.displayPrimaryStats.Add(new ItemDetailStatLine("buildValidationErrors", FormatIds(buildResult.ValidationErrors), "Read-only resolver validation output."));
            }
        }

        private static ItemDetailBuildPreview BuildTrackPreview(
            ItemBuildTrackResult track,
            string countedText)
        {
            return new ItemDetailBuildPreview(
                track.displayName,
                track.progressText,
                $"{countedText} Source placements: {FormatIds(track.SourcePlacementIds)}.",
                track.hasAnyActiveStage);
        }

        private static string BuildCountedText(ItemBuildSynergyItemResult itemBuild)
        {
            if (itemBuild == null)
            {
                return "This placement has no Build result.";
            }

            if (itemBuild.countedInBuild)
            {
                return "This lit placement counts into Build statistics.";
            }

            if (itemBuild.isLightingSource)
            {
                return "Excluded from Build statistics: JuNian lighting source.";
            }

            if (!itemBuild.isLit)
            {
                return "Excluded from Build statistics: isLit=false.";
            }

            return "Excluded from Build statistics: missing valid faMenTag or qiLeiTag.";
        }

        private static string BuildBattleStateText(
            bool basicEffectActive,
            bool coreEffectUnlocked,
            bool coreEffectActive)
        {
            if (basicEffectActive && coreEffectActive)
            {
                return "Basic effect and unlocked core effect are currently fireable by state projection only; no skill release or damage settlement is executed.";
            }

            if (basicEffectActive && coreEffectUnlocked)
            {
                return "Basic effect is active; core effect is unlocked but not currently fireable. No battle settlement is executed.";
            }

            if (basicEffectActive)
            {
                return "Basic effect is active; core effect is locked. No battle settlement is executed.";
            }

            if (coreEffectUnlocked)
            {
                return "Core effect is unlocked but inactive because isLit=false. No battle settlement is executed.";
            }

            return "Basic effect and core effect are inactive by state projection; no skill release or damage settlement is executed.";
        }

        private static string FormatIds(IReadOnlyList<string> ids)
        {
            return ids == null || ids.Count == 0 ? "None" : string.Join("|", ids);
        }

        private static string BuildLightingText(ItemLightingItemResult result)
        {
            if (result.isLightingSource)
            {
                return "点亮源";
            }

            if (result.isDirectLit)
            {
                return "直接点亮";
            }

            if (result.IsRelayLit)
            {
                return "相邻接亮";
            }

            return "未点亮";
        }

        private static string BuildArrayBonusText(ItemArrayBonusItemResult result)
        {
            if (result == null || !result.isOnArrayBonusCell)
            {
                return "未占阵脉";
            }

            return result.isArrayBonusActive
                ? "阵脉生效"
                : "阵脉待亮";
        }

        private static string FormatArrayBonusCells(ItemArrayBonusItemResult result)
        {
            if (result == null || result.OccupiedArrayBonusCellIds.Count == 0)
            {
                return "None";
            }

            return ItemArrayBonusResolver.FormatCellIds(result.OccupiedArrayBonusCellIds);
        }

        private static ItemDetailViewModel Clone(ItemDetailViewModel source)
        {
            if (source == null)
            {
                return null;
            }

            ItemDetailViewModel clone = new()
            {
                itemId = source.itemId,
                placementId = source.placementId,
                displayItemName = source.displayItemName,
                displayRarityName = source.displayRarityName,
                displayFaMenName = source.displayFaMenName,
                displayQiLeiName = source.displayQiLeiName,
                displayShapeName = source.displayShapeName,
                displayItemPower = source.displayItemPower,
                displayTriggerText = source.displayTriggerText,
                displayLightingStatusText = source.displayLightingStatusText,
                displayAwakeningStatusText = source.displayAwakeningStatusText,
                displayArrayBonusStatusText = source.displayArrayBonusStatusText,
                displayOrangeAffix = source.displayOrangeAffix,
                displayFlavorText = source.displayFlavorText,
                iconPlaceholderKey = source.iconPlaceholderKey,
                rarityColorKey = source.rarityColorKey,
                statusFlags = CloneStatusFlags(source.statusFlags),
                displayPrimaryStats = CloneStats(source.displayPrimaryStats),
                displayFixedAffixes = CloneTextLines(source.displayFixedAffixes),
                displayRandomAffixes = CloneTextLines(source.displayRandomAffixes),
                displayBasicEffects = CloneTextLines(source.displayBasicEffects),
                displayCoreEffects = CloneTextLines(source.displayCoreEffects),
                displayFaMenBuilds = CloneBuilds(source.displayFaMenBuilds),
                displayQiLeiBuilds = CloneBuilds(source.displayQiLeiBuilds),
                displayPlacementTips = CloneTextLines(source.displayPlacementTips),
                lightingPreview = CloneLightingPreview(source.lightingPreview),
                buildPreview = CloneBuildPreview(source.buildPreview),
                awakeningPreview = CloneAwakeningPreview(source.awakeningPreview),
                acquirePreview = CloneAcquirePreview(source.acquirePreview),
                upgradePreview = CloneUpgradePreview(source.upgradePreview),
                battleEffectPreview = CloneBattleEffectPreview(source.battleEffectPreview)
            };
            return clone;
        }

        private static ItemDetailStatusFlags CloneStatusFlags(ItemDetailStatusFlags source)
        {
            return source == null
                ? new ItemDetailStatusFlags()
                : new ItemDetailStatusFlags
                {
                    placementId = source.placementId,
                    isLit = source.isLit,
                    isLightingSource = source.isLightingSource,
                    isDirectLit = source.isDirectLit,
                    isRelayLit = source.isRelayLit,
                    coreEffectUnlocked = source.coreEffectUnlocked,
                    coreEffectActive = source.coreEffectActive,
                    isOnArrayBonusCell = source.isOnArrayBonusCell,
                    isArrayBonusActive = source.isArrayBonusActive,
                    countedInBuild = source.countedInBuild,
                    litByItemId = source.litByItemId,
                    litDepth = source.litDepth,
                    basicEffectActive = source.basicEffectActive,
                    inputLevel = source.inputLevel,
                    resolvedLevel = source.resolvedLevel,
                    itemLevel = source.itemLevel,
                    unlockedCoreEffectCount = source.unlockedCoreEffectCount,
                    activeCoreEffectCount = source.activeCoreEffectCount,
                    currentAwakeningNodeLevel = source.currentAwakeningNodeLevel,
                    nextAwakeningNodeLevel = source.nextAwakeningNodeLevel
                };
        }

        private static List<ItemDetailStatLine> CloneStats(IReadOnlyList<ItemDetailStatLine> source)
        {
            List<ItemDetailStatLine> clone = new();
            if (source == null)
            {
                return clone;
            }

            foreach (ItemDetailStatLine line in source)
            {
                if (line != null)
                {
                    clone.Add(new ItemDetailStatLine(line.label, line.value, line.hint));
                }
            }

            return clone;
        }

        private static List<ItemDetailTextLine> CloneTextLines(IReadOnlyList<ItemDetailTextLine> source)
        {
            List<ItemDetailTextLine> clone = new();
            if (source == null)
            {
                return clone;
            }

            foreach (ItemDetailTextLine line in source)
            {
                if (line != null)
                {
                    clone.Add(new ItemDetailTextLine(line.title, line.body, line.stateKey));
                }
            }

            return clone;
        }

        private static List<ItemDetailBuildPreview> CloneBuilds(IReadOnlyList<ItemDetailBuildPreview> source)
        {
            List<ItemDetailBuildPreview> clone = new();
            if (source == null)
            {
                return clone;
            }

            foreach (ItemDetailBuildPreview build in source)
            {
                if (build != null)
                {
                    clone.Add(new ItemDetailBuildPreview(
                        build.buildName,
                        build.progressText,
                        build.previewText,
                        build.isActivePreview));
                }
            }

            return clone;
        }

        private static ItemLightingPreview CloneLightingPreview(ItemLightingPreview source)
        {
            return source == null
                ? new ItemLightingPreview()
                : new ItemLightingPreview
                {
                    placementId = source.placementId,
                    isLit = source.isLit,
                    isLightingSource = source.isLightingSource,
                    isDirectLit = source.isDirectLit,
                    isRelayLit = source.isRelayLit,
                    isOnArrayBonusCell = source.isOnArrayBonusCell,
                    isArrayBonusActive = source.isArrayBonusActive,
                    litByItemId = source.litByItemId,
                    litDepth = source.litDepth,
                    basicEffectActive = source.basicEffectActive,
                    previewText = source.previewText
                };
        }

        private static ItemBuildPreview CloneBuildPreview(ItemBuildPreview source)
        {
            return source == null
                ? new ItemBuildPreview()
                : new ItemBuildPreview
                {
                    faMenPreviewText = source.faMenPreviewText,
                    qiLeiPreviewText = source.qiLeiPreviewText,
                    countedInBuild = source.countedInBuild
                };
        }

        private static ItemAwakeningPreview CloneAwakeningPreview(ItemAwakeningPreview source)
        {
            return source == null
                ? new ItemAwakeningPreview()
                : new ItemAwakeningPreview
                {
                    inputLevel = source.inputLevel,
                    resolvedLevel = source.resolvedLevel,
                    itemLevel = source.itemLevel,
                    coreEffectUnlocked = source.coreEffectUnlocked,
                    coreEffectActive = source.coreEffectActive,
                    unlockedCoreEffectCount = source.unlockedCoreEffectCount,
                    activeCoreEffectCount = source.activeCoreEffectCount,
                    currentNodeText = source.currentNodeText,
                    nextNodeText = source.nextNodeText,
                    unlockedNodeLevelsText = source.unlockedNodeLevelsText,
                    activeNodeLevelsText = source.activeNodeLevelsText,
                    unlockedCoreEffectIdsText = source.unlockedCoreEffectIdsText,
                    activeCoreEffectIdsText = source.activeCoreEffectIdsText,
                    lockedCoreEffectIdsText = source.lockedCoreEffectIdsText,
                    validationErrorsText = source.validationErrorsText,
                    readOnlyInputText = source.readOnlyInputText
                };
        }

        private static ItemAcquirePreview CloneAcquirePreview(ItemAcquirePreview source)
        {
            return source == null
                ? new ItemAcquirePreview()
                : new ItemAcquirePreview { sourceHint = source.sourceHint };
        }

        private static ItemUpgradePreview CloneUpgradePreview(ItemUpgradePreview source)
        {
            return source == null
                ? new ItemUpgradePreview()
                : new ItemUpgradePreview
                {
                    upgradeHint = source.upgradeHint,
                    raritySlotHint = source.raritySlotHint,
                    affixSlotHint = source.affixSlotHint
                };
        }

        private static ItemBattleEffectPreview CloneBattleEffectPreview(ItemBattleEffectPreview source)
        {
            return source == null
                ? new ItemBattleEffectPreview()
                : new ItemBattleEffectPreview
                {
                    basicEffectActive = source.basicEffectActive,
                    coreEffectActive = source.coreEffectActive,
                    battleStateText = source.battleStateText
                };
        }

        private static string FormatBool(bool value)
        {
            return value ? "true" : "false";
        }

        private static string FormatNullable(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "None" : value;
        }
    }
}
