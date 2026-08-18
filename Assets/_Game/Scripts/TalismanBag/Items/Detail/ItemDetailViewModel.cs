using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TalismanBag.Items.Build;
using TalismanBag.Items.Build.Qualified;
using TalismanBag.Items.Generation.Potential;

namespace TalismanBag.Items.Detail
{
    public sealed class ItemDetailViewModel
    {
        public string itemId;
        public string itemInstanceId;
        public string baseItemId;
        public string placementId;
        public string rarityKey;
        public string rarityDisplayName;
        public string displayItemName;
        public string displayRarityName;
        public string displayFaMenName;
        public string displayQiLeiName;
        public string displayShapeName;
        public string displayItemPower;
        public List<ItemDetailStatLine> displayPrimaryStats = new();
        public string displayTriggerText;
        public string displayLightingStatusText;
        public string displayAwakeningStatusText;
        public string displayArrayBonusStatusText;
        public ItemDetailStatusFlags statusFlags = new();
        public List<ItemDetailTextLine> displayFixedAffixes = new();
        public List<ItemDetailTextLine> displayRandomAffixes = new();
        public string displayOrangeAffix;
        public List<ItemDetailTextLine> displayBasicEffects = new();
        public List<ItemDetailTextLine> displayCoreEffects = new();
        public List<ItemDetailBuildPreview> displayFaMenBuilds = new();
        public List<ItemDetailBuildPreview> displayQiLeiBuilds = new();
        public List<ItemDetailTextLine> displayPlacementTips = new();
        public string displayFlavorText;
        public string iconPlaceholderKey;
        public string rarityColorKey;
        public ItemLightingPreview lightingPreview = new();
        public ItemBuildPreview buildPreview = new();
        public ItemAwakeningPreview awakeningPreview = new();
        public ItemAcquirePreview acquirePreview = new();
        public ItemUpgradePreview upgradePreview = new();
        public ItemBattleEffectPreview battleEffectPreview = new();
        public ItemSkillMonitorPreview skillMonitorPreview = new();
        public ItemDetailQualifiedBuildTrackProjection qualifiedBuildTrack { get; internal set; } =
            ItemDetailQualifiedBuildTrackProjection.NotApplicable();
        public ItemDetailQualifiedCoreEffectProjection qualifiedCoreEffect { get; internal set; } =
            ItemDetailQualifiedCoreEffectProjection.NotApplicable();
        public List<ItemDetailSectionViewModel> displayPlayerSections = new();
        public List<ItemDetailSectionViewModel> displayDebugSections = new();

        public ItemDetailViewModel Clone()
        {
            return new ItemDetailViewModel
            {
                itemId = itemId,
                itemInstanceId = itemInstanceId,
                baseItemId = baseItemId,
                placementId = placementId,
                rarityKey = rarityKey,
                rarityDisplayName = rarityDisplayName,
                displayItemName = displayItemName,
                displayRarityName = displayRarityName,
                displayFaMenName = displayFaMenName,
                displayQiLeiName = displayQiLeiName,
                displayShapeName = displayShapeName,
                displayItemPower = displayItemPower,
                displayPrimaryStats = CloneStats(displayPrimaryStats),
                displayTriggerText = displayTriggerText,
                displayLightingStatusText = displayLightingStatusText,
                displayAwakeningStatusText = displayAwakeningStatusText,
                displayArrayBonusStatusText = displayArrayBonusStatusText,
                statusFlags = CloneStatusFlags(statusFlags),
                displayFixedAffixes = CloneTextLines(displayFixedAffixes),
                displayRandomAffixes = CloneTextLines(displayRandomAffixes),
                displayOrangeAffix = displayOrangeAffix,
                displayBasicEffects = CloneTextLines(displayBasicEffects),
                displayCoreEffects = CloneTextLines(displayCoreEffects),
                displayFaMenBuilds = CloneBuilds(displayFaMenBuilds),
                displayQiLeiBuilds = CloneBuilds(displayQiLeiBuilds),
                displayPlacementTips = CloneTextLines(displayPlacementTips),
                displayFlavorText = displayFlavorText,
                iconPlaceholderKey = iconPlaceholderKey,
                rarityColorKey = rarityColorKey,
                lightingPreview = CloneLightingPreview(lightingPreview),
                buildPreview = CloneBuildPreview(buildPreview),
                awakeningPreview = CloneAwakeningPreview(awakeningPreview),
                acquirePreview = CloneAcquirePreview(acquirePreview),
                upgradePreview = CloneUpgradePreview(upgradePreview),
                battleEffectPreview = CloneBattleEffectPreview(battleEffectPreview),
                skillMonitorPreview = CloneSkillMonitorPreview(skillMonitorPreview),
                qualifiedBuildTrack = qualifiedBuildTrack?.Clone()
                    ?? ItemDetailQualifiedBuildTrackProjection.NotApplicable(),
                qualifiedCoreEffect = qualifiedCoreEffect?.Clone()
                    ?? ItemDetailQualifiedCoreEffectProjection.NotApplicable(),
                displayPlayerSections = CloneSections(displayPlayerSections),
                displayDebugSections = CloneSections(displayDebugSections)
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

        private static List<ItemDetailSectionViewModel> CloneSections(IReadOnlyList<ItemDetailSectionViewModel> source)
        {
            List<ItemDetailSectionViewModel> clone = new();
            if (source == null)
            {
                return clone;
            }

            foreach (ItemDetailSectionViewModel section in source)
            {
                if (section != null)
                {
                    clone.Add(new ItemDetailSectionViewModel(
                        section.title,
                        section.body,
                        section.stateKey,
                        section.keepWhenEmpty,
                        section.coreEffectRowStates));
                }
            }

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

        private static ItemSkillMonitorPreview CloneSkillMonitorPreview(ItemSkillMonitorPreview source)
        {
            return source == null
                ? new ItemSkillMonitorPreview()
                : new ItemSkillMonitorPreview
                {
                    selectedMainBuildId = source.selectedMainBuildId,
                    selectedMainBuildFound = source.selectedMainBuildFound,
                    selectedMainBuildCountText = source.selectedMainBuildCountText,
                    selectedMainBuildStageText = source.selectedMainBuildStageText,
                    validationErrorsText = source.validationErrorsText,
                    previewText = source.previewText,
                    slotPreviewLines = CloneTextLines(source.slotPreviewLines)
                };
        }
    }

    public enum ItemDetailQualifiedBuildProjectionStatus
    {
        Complete = 0,
        Unknown = 1,
        NotApplicable = 2
    }

    public sealed class ItemDetailQualifiedBuildTrackProjection
    {
        private readonly ReadOnlyCollection<ItemDetailQualifiedBuildTrackRow> trackRows;

        public ItemDetailQualifiedBuildTrackProjection(
            ItemDetailQualifiedBuildProjectionStatus status,
            ItemInstanceQualifiedBuildFactCompleteness completeness,
            string itemInstanceId,
            string baseItemId,
            string placementId,
            ItemInstanceQualifiedBuildLocation location,
            ItemBuildQualification? buildQualification,
            ItemInstanceQualifiedBuildBooleanFact isLitFact,
            ItemInstanceQualifiedBuildBooleanFact sourceIsCountedFact,
            ItemInstanceQualifiedBuildBooleanFact qualifiedIsCountedFact,
            string eligibleFaMenBuildId,
            string eligibleQiLeiBuildId,
            int? faMenBuildCount,
            int? qiLeiBuildCount,
            int? faMenActiveStagePieceCount,
            int? qiLeiActiveStagePieceCount,
            IEnumerable<ItemDetailQualifiedBuildTrackRow> trackRows,
            string diagnosticCode,
            string safeUnavailableReason,
            string sourceQualifiedBuildCanonicalSignature)
        {
            this.status = status;
            this.completeness = completeness;
            this.itemInstanceId = Normalize(itemInstanceId);
            this.baseItemId = Normalize(baseItemId);
            this.placementId = Normalize(placementId);
            this.location = location;
            this.buildQualification = buildQualification;
            this.isLitFact = isLitFact;
            this.sourceIsCountedFact = sourceIsCountedFact;
            this.qualifiedIsCountedFact = qualifiedIsCountedFact;
            this.eligibleFaMenBuildId = Normalize(eligibleFaMenBuildId);
            this.eligibleQiLeiBuildId = Normalize(eligibleQiLeiBuildId);
            this.faMenBuildCount = faMenBuildCount;
            this.qiLeiBuildCount = qiLeiBuildCount;
            this.faMenActiveStagePieceCount = faMenActiveStagePieceCount;
            this.qiLeiActiveStagePieceCount = qiLeiActiveStagePieceCount;
            this.trackRows = Array.AsReadOnly((trackRows ??
                    Array.Empty<ItemDetailQualifiedBuildTrackRow>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.trackKind)
                .ThenBy(value => value.buildId, StringComparer.Ordinal)
                .ToArray());
            this.diagnosticCode = Normalize(diagnosticCode);
            this.safeUnavailableReason = safeUnavailableReason ?? string.Empty;
            this.sourceQualifiedBuildCanonicalSignature =
                Normalize(sourceQualifiedBuildCanonicalSignature);
            canonicalSignature = ItemDetailQualifiedBuildCanonical.Sha256(BuildCanonicalPayload());
        }

        public ItemDetailQualifiedBuildProjectionStatus status { get; }
        public ItemInstanceQualifiedBuildFactCompleteness completeness { get; }
        public string itemInstanceId { get; }
        public string baseItemId { get; }
        public string placementId { get; }
        public ItemInstanceQualifiedBuildLocation location { get; }
        public ItemBuildQualification? buildQualification { get; }
        public ItemInstanceQualifiedBuildBooleanFact isLitFact { get; }
        public ItemInstanceQualifiedBuildBooleanFact sourceIsCountedFact { get; }
        public ItemInstanceQualifiedBuildBooleanFact qualifiedIsCountedFact { get; }
        public string eligibleFaMenBuildId { get; }
        public string eligibleQiLeiBuildId { get; }
        public int? faMenBuildCount { get; }
        public int? qiLeiBuildCount { get; }
        public int? faMenActiveStagePieceCount { get; }
        public int? qiLeiActiveStagePieceCount { get; }
        public IReadOnlyList<ItemDetailQualifiedBuildTrackRow> TrackRows => trackRows;
        public string diagnosticCode { get; }
        public string safeUnavailableReason { get; }
        public string sourceQualifiedBuildCanonicalSignature { get; }
        public string canonicalSignature { get; }

        public ItemDetailQualifiedBuildTrackProjection Clone() =>
            new(
                status,
                completeness,
                itemInstanceId,
                baseItemId,
                placementId,
                location,
                buildQualification,
                isLitFact,
                sourceIsCountedFact,
                qualifiedIsCountedFact,
                eligibleFaMenBuildId,
                eligibleQiLeiBuildId,
                faMenBuildCount,
                qiLeiBuildCount,
                faMenActiveStagePieceCount,
                qiLeiActiveStagePieceCount,
                trackRows,
                diagnosticCode,
                safeUnavailableReason,
                sourceQualifiedBuildCanonicalSignature);

        public static ItemDetailQualifiedBuildTrackProjection NotApplicable(
            string baseItemId = null,
            string diagnosticCode = "DETAIL_QUALIFIED_NOT_APPLICABLE") =>
            new(
                ItemDetailQualifiedBuildProjectionStatus.NotApplicable,
                ItemInstanceQualifiedBuildFactCompleteness.NotApplicable,
                null,
                baseItemId,
                null,
                ItemInstanceQualifiedBuildLocation.Unknown,
                null,
                ItemInstanceQualifiedBuildBooleanFact.NotApplicable,
                ItemInstanceQualifiedBuildBooleanFact.NotApplicable,
                ItemInstanceQualifiedBuildBooleanFact.NotApplicable,
                null,
                null,
                null,
                null,
                null,
                null,
                Array.Empty<ItemDetailQualifiedBuildTrackRow>(),
                diagnosticCode,
                string.Empty,
                null);

        private string BuildCanonicalPayload()
        {
            StringBuilder builder = new();
            ItemDetailQualifiedBuildCanonical.Append(builder, "status", status.ToString());
            ItemDetailQualifiedBuildCanonical.Append(builder, "completeness", completeness.ToString());
            ItemDetailQualifiedBuildCanonical.AppendOptional(builder, "itemInstanceId", itemInstanceId);
            ItemDetailQualifiedBuildCanonical.AppendOptional(builder, "baseItemId", baseItemId);
            ItemDetailQualifiedBuildCanonical.AppendOptional(builder, "placementId", placementId);
            ItemDetailQualifiedBuildCanonical.Append(builder, "location", location.ToString());
            ItemDetailQualifiedBuildCanonical.AppendNullableEnum(
                builder, "buildQualification", buildQualification);
            ItemDetailQualifiedBuildCanonical.Append(builder, "isLitFact", isLitFact.ToString());
            ItemDetailQualifiedBuildCanonical.Append(
                builder, "sourceIsCountedFact", sourceIsCountedFact.ToString());
            ItemDetailQualifiedBuildCanonical.Append(
                builder, "qualifiedIsCountedFact", qualifiedIsCountedFact.ToString());
            ItemDetailQualifiedBuildCanonical.AppendOptional(
                builder, "eligibleFaMenBuildId", eligibleFaMenBuildId);
            ItemDetailQualifiedBuildCanonical.AppendOptional(
                builder, "eligibleQiLeiBuildId", eligibleQiLeiBuildId);
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "faMenBuildCount", faMenBuildCount);
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "qiLeiBuildCount", qiLeiBuildCount);
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "faMenActiveStagePieceCount", faMenActiveStagePieceCount);
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "qiLeiActiveStagePieceCount", qiLeiActiveStagePieceCount);
            ItemDetailQualifiedBuildCanonical.Append(
                builder, "trackRowCount", trackRows.Count.ToString(CultureInfo.InvariantCulture));
            foreach (ItemDetailQualifiedBuildTrackRow row in trackRows)
            {
                ItemDetailQualifiedBuildCanonical.Append(builder, "track", row.BuildCanonicalPayload());
            }
            ItemDetailQualifiedBuildCanonical.AppendOptional(builder, "diagnosticCode", diagnosticCode);
            ItemDetailQualifiedBuildCanonical.Append(
                builder, "safeUnavailableReason", safeUnavailableReason);
            ItemDetailQualifiedBuildCanonical.AppendOptional(
                builder, "sourceQualifiedBuildCanonicalSignature",
                sourceQualifiedBuildCanonicalSignature);
            return builder.ToString();
        }

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    public sealed class ItemDetailQualifiedBuildTrackRow
    {
        private readonly ReadOnlyCollection<ItemDetailQualifiedBuildStageRow> stageRows;

        public ItemDetailQualifiedBuildTrackRow(
            string buildId,
            ItemBuildTrackKind trackKind,
            string stableTag,
            string displayName,
            ItemInstanceQualifiedBuildFactCompleteness completeness,
            int? qualifiedItemCount,
            int? maxPieceCount,
            int? activeStagePieceCount,
            int? nextStagePieceCount,
            IEnumerable<ItemDetailQualifiedBuildStageRow> stageRows)
        {
            this.buildId = buildId ?? string.Empty;
            this.trackKind = trackKind;
            this.stableTag = stableTag ?? string.Empty;
            this.displayName = displayName ?? string.Empty;
            this.completeness = completeness;
            this.qualifiedItemCount = qualifiedItemCount;
            this.maxPieceCount = maxPieceCount;
            this.activeStagePieceCount = activeStagePieceCount;
            this.nextStagePieceCount = nextStagePieceCount;
            this.stageRows = Array.AsReadOnly((stageRows ??
                    Array.Empty<ItemDetailQualifiedBuildStageRow>())
                .Where(value => value != null)
                .Select(value => value.Clone())
                .OrderBy(value => value.stagePieceCount)
                .ToArray());
        }

        public string buildId { get; }
        public ItemBuildTrackKind trackKind { get; }
        public string stableTag { get; }
        public string displayName { get; }
        public ItemInstanceQualifiedBuildFactCompleteness completeness { get; }
        public int? qualifiedItemCount { get; }
        public int? maxPieceCount { get; }
        public int? activeStagePieceCount { get; }
        public int? nextStagePieceCount { get; }
        public IReadOnlyList<ItemDetailQualifiedBuildStageRow> StageRows => stageRows;

        public ItemDetailQualifiedBuildTrackRow Clone() =>
            new(
                buildId,
                trackKind,
                stableTag,
                displayName,
                completeness,
                qualifiedItemCount,
                maxPieceCount,
                activeStagePieceCount,
                nextStagePieceCount,
                stageRows);

        internal string BuildCanonicalPayload()
        {
            StringBuilder builder = new();
            ItemDetailQualifiedBuildCanonical.Append(builder, "buildId", buildId);
            ItemDetailQualifiedBuildCanonical.Append(builder, "trackKind", trackKind.ToString());
            ItemDetailQualifiedBuildCanonical.Append(builder, "stableTag", stableTag);
            ItemDetailQualifiedBuildCanonical.Append(builder, "displayName", displayName);
            ItemDetailQualifiedBuildCanonical.Append(builder, "completeness", completeness.ToString());
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "qualifiedItemCount", qualifiedItemCount);
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "maxPieceCount", maxPieceCount);
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "activeStagePieceCount", activeStagePieceCount);
            ItemDetailQualifiedBuildCanonical.AppendNullableInt(
                builder, "nextStagePieceCount", nextStagePieceCount);
            foreach (ItemDetailQualifiedBuildStageRow row in stageRows)
            {
                ItemDetailQualifiedBuildCanonical.Append(builder, "stage", row.BuildCanonicalPayload());
            }
            return builder.ToString();
        }
    }

    public sealed class ItemDetailQualifiedBuildStageRow
    {
        public ItemDetailQualifiedBuildStageRow(
            int stagePieceCount,
            string effectDescription,
            bool isActive)
        {
            this.stagePieceCount = stagePieceCount;
            this.effectDescription = effectDescription ?? string.Empty;
            this.isActive = isActive;
        }

        public int stagePieceCount { get; }
        public string effectDescription { get; }
        public bool isActive { get; }

        public ItemDetailQualifiedBuildStageRow Clone() =>
            new(stagePieceCount, effectDescription, isActive);

        internal string BuildCanonicalPayload()
        {
            StringBuilder builder = new();
            ItemDetailQualifiedBuildCanonical.Append(
                builder, "stagePieceCount", stagePieceCount.ToString(CultureInfo.InvariantCulture));
            ItemDetailQualifiedBuildCanonical.Append(builder, "effectDescription", effectDescription);
            ItemDetailQualifiedBuildCanonical.Append(builder, "isActive", isActive ? "true" : "false");
            return builder.ToString();
        }
    }

    internal static class ItemDetailQualifiedBuildCanonical
    {
        public static void Append(StringBuilder builder, string key, string value)
        {
            string safeKey = key ?? string.Empty;
            string safeValue = value ?? string.Empty;
            builder.Append(safeKey.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeKey).Append('=')
                .Append(safeValue.Length.ToString(CultureInfo.InvariantCulture))
                .Append(':').Append(safeValue).Append('\n');
        }

        public static void AppendOptional(StringBuilder builder, string key, string value)
        {
            Append(builder, key + ".presence", value == null ? "Missing" : "Present");
            if (value != null)
            {
                Append(builder, key, value);
            }
        }

        public static void AppendNullableInt(StringBuilder builder, string key, int? value)
        {
            Append(builder, key + ".presence", value.HasValue ? "Present" : "Missing");
            if (value.HasValue)
            {
                Append(builder, key, value.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        public static void AppendNullableEnum<T>(StringBuilder builder, string key, T? value)
            where T : struct
        {
            Append(builder, key + ".presence", value.HasValue ? "Present" : "Missing");
            if (value.HasValue)
            {
                Append(builder, key, value.Value.ToString());
            }
        }

        public static string Sha256(string payload)
        {
            using SHA256 sha = SHA256.Create();
            byte[] digest = sha.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
            return "sha256:" + BitConverter.ToString(digest).Replace("-", string.Empty).ToLowerInvariant();
        }
    }

    public enum ItemDetailCoreEffectRowState
    {
        Locked = 0,
        UnlockedInactive = 1,
        Active = 2
    }

    public sealed class ItemDetailSectionViewModel
    {
        public string title;
        public string body;
        public string stateKey;
        public bool keepWhenEmpty;
        public List<ItemDetailCoreEffectRowState> coreEffectRowStates = new();

        public ItemDetailSectionViewModel()
        {
        }

        public ItemDetailSectionViewModel(
            string title,
            string body,
            string stateKey = "",
            bool keepWhenEmpty = true,
            IEnumerable<ItemDetailCoreEffectRowState> coreEffectRowStates = null)
        {
            this.title = title;
            this.body = body;
            this.stateKey = stateKey;
            this.keepWhenEmpty = keepWhenEmpty;
            this.coreEffectRowStates = (coreEffectRowStates ?? Array.Empty<ItemDetailCoreEffectRowState>()).ToList();
        }
    }

    public sealed class ItemDetailStatLine
    {
        public string label;
        public string value;
        public string hint;

        public ItemDetailStatLine()
        {
        }

        public ItemDetailStatLine(string label, string value, string hint = "")
        {
            this.label = label;
            this.value = value;
            this.hint = hint;
        }
    }

    public sealed class ItemDetailTextLine
    {
        public string title;
        public string body;
        public string stateKey;

        public ItemDetailTextLine()
        {
        }

        public ItemDetailTextLine(string title, string body, string stateKey = "")
        {
            this.title = title;
            this.body = body;
            this.stateKey = stateKey;
        }
    }

    public sealed class ItemDetailBuildPreview
    {
        public string buildName;
        public string progressText;
        public string previewText;
        public bool isActivePreview;

        public ItemDetailBuildPreview()
        {
        }

        public ItemDetailBuildPreview(
            string buildName,
            string progressText,
            string previewText,
            bool isActivePreview)
        {
            this.buildName = buildName;
            this.progressText = progressText;
            this.previewText = previewText;
            this.isActivePreview = isActivePreview;
        }
    }

    public sealed class ItemDetailStatusFlags
    {
        public string placementId;
        public bool isLit;
        public bool isLightingSource;
        public bool isDirectLit;
        public bool isRelayLit;
        public bool coreEffectUnlocked;
        public bool coreEffectActive;
        public bool isOnArrayBonusCell;
        public bool isArrayBonusActive;
        public bool countedInBuild;
        public string litByItemId;
        public int litDepth = -1;
        public bool basicEffectActive;
        public int inputLevel;
        public int resolvedLevel;
        public int itemLevel;
        public int unlockedCoreEffectCount;
        public int activeCoreEffectCount;
        public int currentAwakeningNodeLevel;
        public int nextAwakeningNodeLevel;
    }

    public interface IItemDetailViewModelProvider
    {
        IReadOnlyList<ItemDetailListEntry> GetItemList();
        ItemDetailViewModel GetDetailViewModel(string itemId);
    }

    public sealed class ItemDetailListEntry
    {
        public string itemId;
        public string displayItemName;

        public ItemDetailListEntry()
        {
        }

        public ItemDetailListEntry(string itemId, string displayItemName)
        {
            this.itemId = itemId;
            this.displayItemName = displayItemName;
        }
    }

    public interface IItemLightingPreviewProvider
    {
        ItemLightingPreview GetLightingPreview(string itemId);
    }

    public interface IItemBuildPreviewProvider
    {
        ItemBuildPreview GetBuildPreview(string itemId);
    }

    public interface IItemAwakeningPreviewProvider
    {
        ItemAwakeningPreview GetAwakeningPreview(string itemId);
    }

    public interface IItemAcquirePreviewProvider
    {
        ItemAcquirePreview GetAcquirePreview(string itemId);
    }

    public interface IItemUpgradePreviewProvider
    {
        ItemUpgradePreview GetUpgradePreview(string itemId);
    }

    public interface IItemBattleEffectPreviewProvider
    {
        ItemBattleEffectPreview GetBattleEffectPreview(string itemId);
    }

    public sealed class ItemLightingPreview
    {
        public string placementId;
        public bool isLit;
        public bool isLightingSource;
        public bool isDirectLit;
        public bool isRelayLit;
        public bool isOnArrayBonusCell;
        public bool isArrayBonusActive;
        public string litByItemId;
        public int litDepth = -1;
        public bool basicEffectActive;
        public string previewText;
    }

    public sealed class ItemBuildPreview
    {
        public string faMenPreviewText;
        public string qiLeiPreviewText;
        public bool countedInBuild;
    }

    public sealed class ItemAwakeningPreview
    {
        public int inputLevel;
        public int resolvedLevel;
        public int itemLevel;
        public bool coreEffectUnlocked;
        public bool coreEffectActive;
        public int unlockedCoreEffectCount;
        public int activeCoreEffectCount;
        public string currentNodeText;
        public string nextNodeText;
        public string unlockedNodeLevelsText;
        public string activeNodeLevelsText;
        public string unlockedCoreEffectIdsText;
        public string activeCoreEffectIdsText;
        public string lockedCoreEffectIdsText;
        public string validationErrorsText;
        public string readOnlyInputText;
    }

    public sealed class ItemAcquirePreview
    {
        public string sourceHint;
    }

    public sealed class ItemUpgradePreview
    {
        public string upgradeHint;
        public string raritySlotHint;
        public string affixSlotHint;
    }

    public sealed class ItemBattleEffectPreview
    {
        public bool basicEffectActive;
        public bool coreEffectActive;
        public string battleStateText;
    }

    public sealed class ItemSkillMonitorPreview
    {
        public string selectedMainBuildId;
        public bool selectedMainBuildFound;
        public string selectedMainBuildCountText;
        public string selectedMainBuildStageText;
        public string validationErrorsText;
        public string previewText;
        public List<ItemDetailTextLine> slotPreviewLines = new();
    }
}
