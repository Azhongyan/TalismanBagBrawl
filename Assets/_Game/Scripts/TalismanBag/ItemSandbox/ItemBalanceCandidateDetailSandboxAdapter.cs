using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Detail;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Projection;
using TalismanBag.Items.Generation.Stats;
using TalismanBag.Items.InnerCatalog;

namespace TalismanBag.ItemSandbox
{
    public enum ItemBalanceCandidateDetailRequestStatus
    {
        Success = 0,
        InvalidRequest = 1,
        ProfileMissing = 2,
        OrdinaryGenerationExcluded = 3,
        PreviewFailed = 4,
        ProjectionFailed = 5,
        DetailProjectionFailed = 6
    }

    public sealed class ItemBalanceCandidateDetailRequest
    {
        public string baseItemId;
        public string rarityKey;
        public string rootSeedText;
    }

    public sealed class ItemBalanceCandidateDetailResult
    {
        internal ItemBalanceCandidateDetailResult(
            ItemBalanceCandidateDetailRequestStatus status,
            string baseItemId,
            ItemInstanceRarity rarity,
            long rootSeed,
            ItemBalancePreviewResult preview,
            ItemDetailInstanceProjectionResult detailProjection,
            ItemDetailViewModel viewModel,
            IEnumerable<string> validationErrors,
            int candidateStatRangeCount,
            IEnumerable<KeyValuePair<string, int>> coreUnlockLevels = null)
        {
            this.status = status;
            this.baseItemId = Normalize(baseItemId);
            this.rarity = rarity;
            this.rootSeed = rootSeed;
            this.preview = preview;
            this.detailProjection = detailProjection;
            this.viewModel = viewModel;
            ValidationErrors = Array.AsReadOnly((validationErrors ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray());
            this.candidateStatRangeCount = Math.Max(0, candidateStatRangeCount);
            CoreUnlockLevels = new ReadOnlyDictionary<string, int>(
                (coreUnlockLevels ?? Array.Empty<KeyValuePair<string, int>>())
                .Where(value => !string.IsNullOrWhiteSpace(value.Key))
                .GroupBy(value => value.Key.Trim(), StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => Math.Max(1, group.First().Value),
                    StringComparer.Ordinal));
        }

        public bool isSuccess => status == ItemBalanceCandidateDetailRequestStatus.Success
            && preview?.isSuccess == true
            && detailProjection?.isSuccess == true
            && viewModel != null
            && ValidationErrors.Count == 0;
        public ItemBalanceCandidateDetailRequestStatus status { get; }
        public string baseItemId { get; }
        public ItemInstanceRarity rarity { get; }
        public long rootSeed { get; }
        public ItemBalancePreviewResult preview { get; }
        public ItemDetailInstanceProjectionResult detailProjection { get; }
        public ItemDetailViewModel viewModel { get; }
        public IReadOnlyList<string> ValidationErrors { get; }
        public int candidateStatRangeCount { get; }
        public IReadOnlyDictionary<string, int> CoreUnlockLevels { get; }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }

    /// <summary>
    /// Dev-only, read-only adapter for BALANCE_CANDIDATE previews.
    /// It owns no roll rules and writes no candidate, inventory, save, reward, battle, or runtime state.
    /// </summary>
    public sealed class ItemBalanceCandidateDetailSandboxAdapter
    {
        public const string BalanceCandidate = "BALANCE_CANDIDATE";
        public const string Editable = "EDITABLE";
        public const string NotLiveLocked = "NOT_LIVE_LOCKED";
        public const string NotBattleConnected = "NOT_BATTLE_CONNECTED";
        private const string BuildTitleHex = "F2EDE2";
        private const string BuildActiveHex = "87B66A";
        private const string BuildInactiveHex = "8A8A8A";

        private readonly ItemBalanceWorkbenchCatalog workbenchCatalog;
        private readonly IItemDetailViewModelProvider catalogProvider;
        private readonly ItemBalanceCompiledData compiled;
        private readonly ReadOnlyCollection<string> candidateBaseItemIds;
        private readonly Dictionary<string, ItemBalanceCandidateDetailResult> cache =
            new(StringComparer.Ordinal);

        public ItemBalanceCandidateDetailSandboxAdapter(
            ItemBalanceWorkbenchCatalog workbenchCatalog,
            IItemDetailViewModelProvider catalogProvider)
        {
            this.workbenchCatalog = workbenchCatalog;
            this.catalogProvider = catalogProvider;
            compiled = workbenchCatalog == null ? null : ItemBalanceWorkbenchCompiler.Compile(workbenchCatalog);
            candidateBaseItemIds = Array.AsReadOnly((workbenchCatalog?.profiles
                ?? new List<ItemBalanceProfile>())
                .Where(profile => profile != null
                    && ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(profile.baseItemId))
                .Select(profile => Normalize(profile.baseItemId))
                .Where(value => value.Length > 0)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }

        public IReadOnlyList<string> CandidateBaseItemIds => candidateBaseItemIds;
        public int CachedRequestCount => cache.Count;

        public IReadOnlyList<ItemDetailBuildPreview> BuildTrackStagePreview(
            string stableTag,
            bool faMen,
            int currentCount)
        {
            return BuildRuntimeTrackBuilds(workbenchCatalog, Normalize(stableTag), faMen, Math.Max(0, currentCount));
        }

        public ItemBalanceCandidateDetailResult Request(ItemBalanceCandidateDetailRequest request)
        {
            if (request == null)
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.InvalidRequest,
                    string.Empty, default, 0L, "REQUEST_NULL: Candidate request is null.");
            }

            string baseItemId = Normalize(request.baseItemId);
            string rarityKey = Normalize(request.rarityKey).ToLowerInvariant();
            string rootSeedText = Normalize(request.rootSeedText);
            if (baseItemId.Length == 0)
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.InvalidRequest,
                    baseItemId, default, 0L, "BASE_ITEM_ID_EMPTY: baseItemId is required.");
            }

            if (!ItemInstanceRarityCatalog.TryParseStableKey(rarityKey, out ItemInstanceRarity rarity))
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.InvalidRequest,
                    baseItemId, default, 0L,
                    $"RARITY_INVALID: '{rarityKey}' is not white/green/blue/purple/orange.");
            }

            if (!long.TryParse(rootSeedText, NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out long rootSeed))
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.InvalidRequest,
                    baseItemId, rarity, 0L,
                    $"ROOT_SEED_INVALID: '{rootSeedText}' is not a signed 64-bit integer.");
            }

            if (!ItemRarityInstanceFoundation.IsOrdinaryBaseItemId(baseItemId))
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.OrdinaryGenerationExcluded,
                    baseItemId, rarity, rootSeed,
                    $"BASE_ITEM_NOT_ORDINARY: '{baseItemId}' is excluded from ordinary candidate generation.");
            }

            if (workbenchCatalog == null || compiled == null)
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.InvalidRequest,
                    baseItemId, rarity, rootSeed,
                    "WORKBENCH_UNAVAILABLE: Candidate catalog or compiled schemas are unavailable.");
            }

            ItemBalanceProfile profile = workbenchCatalog.FindProfile(baseItemId);
            if (profile == null)
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.ProfileMissing,
                    baseItemId, rarity, rootSeed,
                    $"PROFILE_MISSING: Candidate profile '{baseItemId}' was not found.");
            }

            string key = baseItemId + "|" + rarity.ToStableKey() + "|"
                + rootSeed.ToString(CultureInfo.InvariantCulture);
            if (cache.TryGetValue(key, out ItemBalanceCandidateDetailResult cached))
            {
                return cached;
            }

            ItemBalancePreviewResult preview = ItemBalanceWorkbenchCompiler.Preview(
                workbenchCatalog, compiled, baseItemId, rarity, rootSeed);
            if (preview?.isSuccess != true || preview.RollResult?.snapshot == null
                || preview.ProjectionResult?.snapshot == null)
            {
                string[] errors = preview?.Errors?.Count > 0
                    ? preview.Errors.ToArray()
                    : new[] { "PREVIEW_FAILED: Workbench preview did not produce a projection." };
                return Failure(ItemBalanceCandidateDetailRequestStatus.PreviewFailed,
                    baseItemId, rarity, rootSeed, errors);
            }

            ItemInstanceProjectionSetResult setResult = ItemInstanceProjectionProvider.ProjectSet(
                new ItemInstanceProjectionSetRequest(new[] { preview.RollResult.snapshot }));
            if (!setResult.isSuccess || setResult.snapshot == null)
            {
                string[] errors = setResult.ValidationErrors
                    .Select(error => error.code + ": " + error.message)
                    .ToArray();
                return Failure(ItemBalanceCandidateDetailRequestStatus.ProjectionFailed,
                    baseItemId, rarity, rootSeed, errors);
            }

            ItemDetailInstanceDataAdapter detailAdapter = new(
                catalogProvider,
                setResult.snapshot,
                compiled.StatSchema,
                compiled.AffixSchema);
            ItemDetailInstanceProjectionResult detail = detailAdapter.Project(
                new ItemDetailInstanceProjectionInput
                {
                    contextKind = ItemDetailProjectionContextKind.GeneratedInstancePreview,
                    itemInstanceId = preview.ProjectionResult.snapshot.itemInstanceId
                });
            if (!detail.isSuccess || detail.viewModel == null)
            {
                return Failure(ItemBalanceCandidateDetailRequestStatus.DetailProjectionFailed,
                    baseItemId, rarity, rootSeed,
                    "DETAIL_PROJECTION_FAILED: " + (detail?.diagnostic ?? "No detail result."));
            }

            ItemDetailViewModel model = AdaptCandidateDetail(
                detail.viewModel,
                preview.ProjectionResult.snapshot,
                compiled.StatSchema,
                compiled.AffixSchema,
                profile,
                workbenchCatalog,
                out int rangeCount);
            ItemBalanceCandidateDetailResult result = new(
                ItemBalanceCandidateDetailRequestStatus.Success,
                baseItemId,
                rarity,
                rootSeed,
                preview,
                detail,
                model,
                Array.Empty<string>(),
                rangeCount,
                (profile.coreCandidates ?? new List<ItemBalanceCoreCandidate>())
                    .Where(value => value != null && !string.IsNullOrWhiteSpace(value.coreEffectId))
                    .Select(value => new KeyValuePair<string, int>(value.coreEffectId, value.unlockLevel)));
            cache[key] = result;
            return result;
        }

        private static ItemDetailViewModel AdaptCandidateDetail(
            ItemDetailViewModel source,
            ItemInstanceProjectionContractSnapshot projection,
            ItemStatRangeSchemaSnapshot statSchema,
            ItemAffixPoolAndRangeSchemaSnapshot affixSchema,
            ItemBalanceProfile profile,
            ItemBalanceWorkbenchCatalog workbench,
            out int rangeCount)
        {
            ItemDetailViewModel model = source.Clone();
            ItemBalanceRarityVersion rarityVersion = profile?.FindVersion(projection.rarity);
            model.displayItemPower = Math.Max(1L, rarityVersion?.candidateItemPower ?? 0L)
                .ToString(CultureInfo.InvariantCulture);
            List<ItemDetailStatLine> stats = new();
            List<string> statDebug = new();
            rangeCount = 0;
            foreach (ItemInstanceProjectionStatSnapshot stat in (projection.Stats
                         ?? Array.Empty<ItemInstanceProjectionStatSnapshot>())
                     .OrderBy(value => StatDisplayOrder(value?.statId, profile))
                     .ThenBy(value => Normalize(value?.statId), StringComparer.Ordinal))
            {
                ItemStatQueryResult<ItemStatDefinitionSnapshot> definitionQuery =
                    statSchema?.QueryStatDefinition(stat.statId);
                ItemStatDefinitionSnapshot definition = definitionQuery?.isSuccess == true
                    ? definitionQuery.value
                    : null;
                ItemStatQueryResult<ItemRarityStatRangeSnapshot> rangeQuery =
                    statSchema?.QueryRarityRange(projection.baseItemId, stat.statId, projection.rarity);
                ItemRarityStatRangeSnapshot range = rangeQuery?.isSuccess == true
                    ? rangeQuery.value
                    : null;
                string label = string.IsNullOrWhiteSpace(definition?.displayName)
                    ? stat.statId
                    : definition.displayName;
                string finalValue = ItemDetailPresentationFormatter.FormatStat(stat.rawUnits, definition);
                if (range != null)
                {
                    rangeCount++;
                }

                string rangeText = range == null
                    ? "UNAVAILABLE"
                    : ItemDetailPresentationFormatter.FormatStatRange(range.minUnits, range.maxUnits, definition);
                stats.Add(new ItemDetailStatLine(label, finalValue, string.Empty));
                statDebug.Add(stat.statId
                    + ":rawUnits=" + stat.rawUnits.ToString(CultureInfo.InvariantCulture)
                    + ":formattedValue=" + finalValue
                    + ":candidateRange=" + rangeText);
            }

            model.displayPrimaryStats = stats;
            ItemBalanceCoreCandidate ultimate = profile?.coreCandidates?.FirstOrDefault(value => value?.isUltimate == true);
            model.displayOrangeAffix = ultimate != null && projection.rarity == ItemInstanceRarity.Orange
                ? FormatCoreEffectBody(ultimate, true)
                : string.Empty;
            model.displayLightingStatusText = NotBattleConnected;
            model.displayArrayBonusStatusText = NotBattleConnected;
            model.displayAwakeningStatusText = "潜力可见；养成开窍未接入";
            model.statusFlags = new ItemDetailStatusFlags();
            if (model.buildPreview != null)
            {
                model.buildPreview.countedInBuild = false;
            }

            ItemCandidateDisplayProfile display = profile?.candidateDisplay ?? new ItemCandidateDisplayProfile();
            model.displayTriggerText = string.Empty;
            model.displayLightingStatusText = CleanPlayerText(NonEmpty(display.lightingDescription, "未点亮时不发动，也不计入Build。"));
            model.displayFlavorText = CleanPlayerText(NonEmpty(display.flavorText, profile?.displayName + "的旧物日记。"));
            model.displayShapeName = NonEmpty(display.shapeDescription, model.displayShapeName);
            model.iconPlaceholderKey = NonEmpty(display.iconKey, model.iconPlaceholderKey);
            model.displayBasicEffects = new List<ItemDetailTextLine>
            {
                new("基础效果", CleanPlayerText(NonEmpty(display.basicEffectDescription, "点亮后发动基础效果。")), "Candidate")
            };
            model.displayPlacementTips = new List<ItemDetailTextLine>
            {
                new("推荐摆放", CleanPlayerText(NonEmpty(display.placementRecommendation, "让核心格靠近聚念范围。")), "Candidate")
            };

            model.displayFixedAffixes = profile?.signatureAffix == null
                ? new List<ItemDetailTextLine>()
                : new List<ItemDetailTextLine>
                {
                    new(profile.signatureAffix.displayName,
                        FormatCandidateAffix(profile.signatureAffix, projection.rarity, highlightKeyInfo: true),
                        "SignatureCandidate")
                };
            model.displayRandomAffixes = projection.Affixes
                .Where(value => value.slotKind == ItemAffixSlotKind.Random)
                .Select(value =>
                {
                    ItemCandidateAffixDefinition definition = workbench?.FindCandidateAffix(value.affixId);
                    return definition == null
                        ? new ItemDetailTextLine(value.affixId,
                            ItemDetailPresentationFormatter.FormatAffix(value.rawUnits,
                                affixSchema?.QueryAffixDefinition(value.affixId)?.value), "RandomCandidate")
                        : new ItemDetailTextLine(definition.displayName,
                            FormatCandidateAffix(
                                definition,
                                projection.rarity,
                                value.rawUnits,
                                highlightKeyInfo: true), "RandomCandidate");
                }).ToList();
            if (model.displayRandomAffixes.Count == 0 && profile?.randomAffixes != null)
            {
                model.displayRandomAffixes = profile.randomAffixes.Take(2).Select(value =>
                {
                    ItemCandidateAffixDefinition definition = workbench?.FindCandidateAffix(value.affixId);
                    return new ItemDetailTextLine(definition?.displayName ?? value.affixId,
                        definition == null
                            ? "词条数据未配置"
                            : FormatCandidateAffix(definition, projection.rarity, highlightKeyInfo: true),
                        "PoolCandidate");
                }).ToList();
            }
            string fixedAffixes = FormatTextLines(model.displayFixedAffixes);
            string randomAffixes = FormatTextLines(model.displayRandomAffixes);
            string eligibleCore = Join(projection.EligibleCoreEffectIds);
            string visibleCore = Join(projection.VisibleCoreEffectIds);
            HashSet<string> visibleCoreIdSet = new(projection.VisibleCoreEffectIds, StringComparer.Ordinal);
            List<ItemBalanceCoreCandidate> previewCoreCandidates = (profile?.coreCandidates
                    ?? new List<ItemBalanceCoreCandidate>())
                .Where(value => value != null
                    && !value.isUltimate
                    && !string.IsNullOrWhiteSpace(value.coreEffectId)
                    && visibleCoreIdSet.Contains(value.coreEffectId))
                .OrderBy(value => value.unlockLevel)
                .ThenBy(value => value.coreEffectId, StringComparer.Ordinal)
                .Take(4)
                .ToList();
            model.displayCoreEffects = previewCoreCandidates.Count > 0
                ? previewCoreCandidates.Select(candidate => new ItemDetailTextLine(
                    string.IsNullOrWhiteSpace(candidate.displayName)
                        ? candidate.coreEffectId
                        : candidate.displayName,
                    FormatCoreEffectDetail(candidate),
                    candidate.coreEffectId)).ToList()
                : projection.EligibleCoreEffectIds.Take(4).Select(coreId => new ItemDetailTextLine(
                    coreId,
                    FormatCoreEffectDetail(null),
                    coreId)).ToList();
            model.displayFaMenBuilds = BuildCandidateBuilds(workbench, profile?.faMenTag, true, 0);
            model.displayQiLeiBuilds = BuildCandidateBuilds(workbench, profile?.qiLeiTag, false, 0);
            string qualification = projection.buildQualification.ToString();
            string previewStateText = "临时预览 · 未创建实例 · 未摆放 · 不写存档";
            string hiddenBasicText = model.displayLightingStatusText + "\n" + FormatTextLines(model.displayBasicEffects);
            string statsText = string.Join("\n", stats
                .Select(value => $"{value.label}：{value.value}")
                .Where(value => !string.IsNullOrWhiteSpace(value)));
            string faMenBuildText = BuildQualificationAllows(projection.buildQualification, true)
                ? FormatBuilds(model.displayFaMenBuilds, projection.rarity, true, profile?.faMenTag)
                : string.Empty;
            string qiLeiBuildText = BuildQualificationAllows(projection.buildQualification, false)
                ? FormatBuilds(model.displayQiLeiBuilds, projection.rarity, false, profile?.qiLeiTag)
                : string.Empty;
            model.displayPlayerSections = new List<ItemDetailSectionViewModel>
            {
                HiddenSection("itemPower"),
                HiddenSection("identity"),
                HiddenSection("status"),
                SectionOrHidden("基础属性", statsText, "stats"),
                SectionOrHidden("固定词条", fixedAffixes, "fixedAffix"),
                SectionOrHidden("随机词条", randomAffixes, "randomAffix"),
                string.IsNullOrWhiteSpace(model.displayOrangeAffix)
                    ? HiddenSection("orange")
                    : SectionOrHidden("道痕 / 终极核心", model.displayOrangeAffix, "orange"),
                SectionOrHidden("核心效果", FormatCoreEffectLines(model.displayCoreEffects, true), "coreEffect"),
                SectionOrHidden("法门构筑", faMenBuildText, "famenBuild"),
                SectionOrHidden("器类构筑", qiLeiBuildText, "qileiBuild"),
                HiddenSection("skillMonitor"),
                SectionOrHidden("推荐摆放", FormatTextLines(model.displayPlacementTips), "placement"),
                SectionOrHidden("旧物日记", model.displayFlavorText, "flavor"),
                HiddenSection("trigger"),
                HiddenSection("basic")
            };

            string affixDebug = string.Join(" | ", projection.Affixes.Select(affix =>
                BuildAffixDebugFact(affix, projection.rarity, affixSchema)));
            model.displayDebugSections = new List<ItemDetailSectionViewModel>
            {
                Section("调试 / 实例身份", "contextKind: CandidateInstancePreview"
                    + "\nitemInstanceId: " + projection.itemInstanceId
                    + "\nbaseItemId: " + projection.baseItemId
                    + "\nrarity: " + projection.rarity
                    + "\ngenerationVersion: " + projection.generationVersion.ToString(CultureInfo.InvariantCulture)
                    + "\nrootSeed: " + projection.rootSeed.ToString(CultureInfo.InvariantCulture)
                    + "\ncultivationPotentialProfileId: " + projection.cultivationPotentialProfileId
                    + "\nplayerHiddenPreviewState: " + previewStateText
                    + "\nplayerHiddenItemPower: " + model.displayItemPower),
                Section("调试 / 生成事实", "stats: " + JoinLines(statDebug)
                    + "\naffixes: " + NonEmpty(affixDebug, "None")
                    + "\neligibleCoreEffectIds: " + eligibleCore
                    + "\nvisibleCoreEffectIds: " + visibleCore
                    + "\nBuildQualification: " + qualification
                    + "\nplayerHiddenBasic: " + hiddenBasicText.Replace("\n", " | ")),
                Section("调试 / 校验与边界", "validationErrors: None"
                    + "\nBALANCE_CANDIDATE\nEDITABLE\nNOT_LIVE_LOCKED\nNOT_BATTLE_CONNECTED"
                    + "\nisLit: false\nisDirectLit: false\nisArrayBonusActive: false"
                    + "\ncoreEffectUnlocked: false\ncoreEffectBattleActive: false"
                    + "\nbuildCounted: false\nskillTriggered: false"
                    + "\nselectedMainBuildId: None"
                    + "\nBasicAttack / Build2 / Build4 / Build6: NOT_EXECUTED"
                    + "\n技能载荷只读预览，不执行正式Battle。")
            };
            return model;
        }

        private static string BuildAffixDebugFact(
            ItemInstanceProjectionAffixSnapshot affix,
            ItemInstanceRarity rarity,
            ItemAffixPoolAndRangeSchemaSnapshot schema)
        {
            ItemAffixQueryResult<ItemAffixDefinitionSnapshot> query =
                schema?.QueryAffixDefinition(affix.affixId);
            ItemAffixDefinitionSnapshot definition = query?.isSuccess == true ? query.value : null;
            ItemAffixQueryResult<ItemAffixValueProfileSnapshot> profileQuery =
                schema?.QueryAffixValueProfile(affix.affixValueProfileId);
            ItemAffixValueProfileSnapshot valueProfile = profileQuery?.isSuccess == true ? profileQuery.value : null;
            ItemAffixRarityValueRangeSnapshot range = valueProfile?.RarityRanges
                .FirstOrDefault(value => value.rarity == rarity);
            string candidateRange = range == null
                ? "UNAVAILABLE"
                : ItemDetailPresentationFormatter.FormatAffixRange(range.minUnits, range.maxUnits, definition);
            return affix.slotId + ":" + affix.slotKind + ":" + affix.affixId
                + ":valueProfileId=" + affix.affixValueProfileId
                + ":rawUnits=" + affix.rawUnits.ToString(CultureInfo.InvariantCulture)
                + ":formattedValue=" + ItemDetailPresentationFormatter.FormatAffix(affix.rawUnits, definition)
                + ":candidateRange=" + candidateRange;
        }

        private static int StatDisplayOrder(string statId, ItemBalanceProfile profile)
        {
            string normalized = Normalize(statId);
            if (normalized.Length == 0)
            {
                return 50;
            }

            if (string.Equals(normalized, Normalize(profile?.primaryStatId), StringComparison.Ordinal))
            {
                return 0;
            }

            if (string.Equals(normalized, Normalize(profile?.secondaryStatId), StringComparison.Ordinal))
            {
                return 10;
            }

            if (string.Equals(normalized, "nianCost", StringComparison.Ordinal))
            {
                return 80;
            }

            if (string.Equals(normalized, "cooldown", StringComparison.Ordinal))
            {
                return 99;
            }

            return 50;
        }

        private static List<ItemDetailBuildPreview> BuildCandidateBuilds(
            ItemBalanceWorkbenchCatalog catalog, string stableTag, bool faMen, int currentCount)
        {
            string normalizedTag = Normalize(stableTag);
            return (catalog?.FindBuildStages(stableTag, faMen)
                    ?? Array.Empty<ItemCandidateBuildStageDefinition>())
                .Select(stage => new ItemDetailBuildPreview(
                    FormatBuildPresentationName(normalizedTag, faMen, stage.stagePieceCount),
                    currentCount + "/" + stage.stagePieceCount,
                    FormatRuntimeBuildStage(stage),
                    currentCount >= stage.stagePieceCount))
                .ToList();
        }

        private static List<ItemDetailBuildPreview> BuildRuntimeTrackBuilds(
            ItemBalanceWorkbenchCatalog catalog, string stableTag, bool faMen, int currentCount)
        {
            string normalizedTag = Normalize(stableTag);
            return (catalog?.FindBuildStages(stableTag, faMen)
                    ?? Array.Empty<ItemCandidateBuildStageDefinition>())
                .Select(stage => new ItemDetailBuildPreview(
                    FormatBuildPresentationName(normalizedTag, faMen, stage.stagePieceCount),
                    currentCount + "/" + stage.stagePieceCount,
                    FormatRuntimeBuildStage(stage),
                    currentCount >= stage.stagePieceCount))
                .ToList();
        }

        private static string FormatBuildPresentationName(string stableTag, bool faMen, int stagePieceCount)
        {
            return faMen
                ? ItemSandboxBuildPresentationNames.FaMenSetName(stableTag)
                : ItemSandboxBuildPresentationNames.QiLeiStageName(stableTag, stagePieceCount);
        }

        private static string FormatBuildStageState(int currentCount, int stagePieceCount)
        {
            return currentCount >= stagePieceCount ? "已激活" : "未激活";
        }

        private static string FormatRuntimeBuildStage(ItemCandidateBuildStageDefinition stage)
        {
            ItemCandidateEffectPayload payload = stage?.effectPayload;
            if (payload == null || string.IsNullOrWhiteSpace(payload.effectId))
            {
                return "未配置";
            }

            string valueText = FormatRuntimeValue(payload.valueUnits, payload.valueUnitKey);
            string secondaryText = payload.secondaryValueUnits == 0
                ? string.Empty
                : "；副值 " + FormatRuntimeValue(payload.secondaryValueUnits, payload.valueUnitKey);
            return NonEmpty(payload.description, payload.displayName)
                + "；" + FormatTargetStat(payload.targetStatId)
                + FormatOperation(payload.operation)
                + " " + FormatSignedValue(payload.operation, valueText)
                + secondaryText;
        }

        private static string FormatTargetStat(string statId)
        {
            return statId switch
            {
                "damage" => "伤害",
                "break" => "破盾",
                "guard" => "护势",
                "heal" => "回复",
                "control" => "控制强度",
                "cooldown" => "触发频率/冷却",
                "duration" => "持续时间",
                "nian" => "念力",
                "nianCost" => "耗念",
                "basicEffect" => "基础效果",
                "primaryStat" => "主属性",
                "targetCount" => "目标数量",
                "damage_to_break" => "伤害转破盾",
                "damage_to_guard" => "伤害转护势",
                "break_to_damage" => "破盾转伤害",
                "heal_to_guard" => "回复转护势",
                "cleanse_to_heal" => "净化转回复",
                "debuff_to_heal" => "负面转回复",
                "burn" => "余焰",
                "ember" => "火种",
                "targeting" => "目标锁定",
                "castProgress" => "施法进度",
                _ => FormatUnknownTargetStat(statId)
            };
        }

        private static string FormatUnknownTargetStat(string statId)
        {
            if (string.IsNullOrWhiteSpace(statId))
            {
                return "目标属性";
            }

            string normalized = statId.Trim();
            string[] converted = normalized.Split(new[] { "_to_" }, StringSplitOptions.None);
            if (converted.Length == 2
                && !string.IsNullOrWhiteSpace(converted[0])
                && !string.IsNullOrWhiteSpace(converted[1]))
            {
                return FormatTargetStat(converted[0]) + "转" + FormatTargetStat(converted[1]);
            }

            return "目标属性";
        }

        private static string FormatOperation(ItemCandidateEffectOperation operation)
        {
            return operation switch
            {
                ItemCandidateEffectOperation.AddFlat => "增加",
                ItemCandidateEffectOperation.AddPercent => "提高",
                ItemCandidateEffectOperation.Multiply => "乘算",
                ItemCandidateEffectOperation.ReduceFlat => "降低",
                ItemCandidateEffectOperation.ReducePercent => "降低",
                ItemCandidateEffectOperation.Refund => "返还",
                ItemCandidateEffectOperation.ExtraTarget => "额外目标",
                ItemCandidateEffectOperation.ExtraTrigger => "额外触发",
                ItemCandidateEffectOperation.Convert => "转化",
                ItemCandidateEffectOperation.Override => "覆盖",
                _ => operation.ToString()
            };
        }

        private static string FormatSignedValue(ItemCandidateEffectOperation operation, string valueText)
        {
            return operation switch
            {
                ItemCandidateEffectOperation.ReduceFlat => "-" + valueText,
                ItemCandidateEffectOperation.ReducePercent => "-" + valueText,
                ItemCandidateEffectOperation.Multiply => "×" + valueText,
                ItemCandidateEffectOperation.Convert => valueText,
                ItemCandidateEffectOperation.Override => valueText,
                _ => "+" + valueText
            };
        }

        private static string FormatRuntimeValue(long value, string unit)
        {
            return unit switch
            {
                "basisPoint" => (value / 100d).ToString("0.##", CultureInfo.InvariantCulture) + "%",
                "point" => value.ToString(CultureInfo.InvariantCulture) + "点",
                "flat" => value.ToString(CultureInfo.InvariantCulture),
                "count" => value.ToString(CultureInfo.InvariantCulture) + "次",
                "stack" => value.ToString(CultureInfo.InvariantCulture) + "层",
                "turn" => value.ToString(CultureInfo.InvariantCulture) + "回合",
                _ => value.ToString(CultureInfo.InvariantCulture)
                    + (string.IsNullOrWhiteSpace(unit) ? string.Empty : " " + unit)
            };
        }

        private static string FormatBuilds(
            IEnumerable<ItemDetailBuildPreview> values,
            ItemInstanceRarity rarity,
            bool faMen,
            string stableTag)
        {
            ItemDetailBuildPreview[] previews = (values ?? Array.Empty<ItemDetailBuildPreview>())
                .Where(value => value != null)
                .ToArray();
            if (previews.Length == 0)
            {
                return BuildColoredText(ItemSandboxBuildPresentationNames.ProgressLabel(faMen, stableTag) + "：0/0", BuildActiveHex, true);
            }

            int maxPieceCount = previews.Max(ResolveBuildStageThreshold);
            int currentCount = previews.Max(ResolveBuildCurrentCount);
            string[] rows = previews
                .OrderBy(ResolveBuildStageThreshold)
                .Select(value => FormatBuildStageRow(value, faMen, currentCount, rarity))
                .ToArray();
            string progressLine = BuildColoredText(
                ItemSandboxBuildPresentationNames.ProgressLabel(faMen, stableTag)
                    + "：" + FormatBuildProgress(currentCount, maxPieceCount),
                BuildActiveHex,
                true);
            if (faMen)
            {
                string memberRows = FormatFaMenCandidateMemberRows(stableTag);
                return BuildColoredText(NonEmpty(previews.FirstOrDefault()?.buildName, "未命名法门典藏"), BuildTitleHex, true)
                    + "\n" + progressLine
                    + (string.IsNullOrWhiteSpace(memberRows) ? string.Empty : "\n" + memberRows)
                    + "\n" + string.Join("\n", rows);
            }

            return progressLine
                + "\n" + string.Join("\n", rows);
        }

        private static string FormatFaMenCandidateMemberRows(string stableTag)
        {
            string normalizedTag = NormalizeBuildStableTag(stableTag);
            if (string.IsNullOrWhiteSpace(normalizedTag))
            {
                return string.Empty;
            }

            return string.Join("\n", ItemInnerDataCatalog.AllItems
                .Where(item => item != null
                    && !item.isLightingSource
                    && string.Equals(
                        NormalizeBuildStableTag(item.FaMenKey),
                        normalizedTag,
                        StringComparison.Ordinal))
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .Select(item => BuildColoredText("-" + item.displayName, BuildInactiveHex, false)));
        }

        private static string NormalizeBuildStableTag(string value)
        {
            string text = (value ?? string.Empty).Trim().ToLowerInvariant();
            if (text.StartsWith("famen:", StringComparison.Ordinal))
            {
                text = text.Substring("famen:".Length);
            }
            else if (text.StartsWith("qilei:", StringComparison.Ordinal))
            {
                text = text.Substring("qilei:".Length);
            }

            int colon = text.IndexOf(':');
            return colon >= 0 ? text.Substring(0, colon) : text;
        }

        private static string FormatBuildStageRow(
            ItemDetailBuildPreview value,
            bool faMen,
            int currentCount,
            ItemInstanceRarity rarity)
        {
            int threshold = ResolveBuildStageThreshold(value);
            bool isActive = currentCount >= threshold;
            string label = FormatBuildStageName(value);
            string detailText = value?.previewText;
            if (!faMen
                && !string.IsNullOrWhiteSpace(value?.buildName)
                && !string.Equals(value.buildName, label, StringComparison.Ordinal))
            {
                detailText = value.buildName + "：" + NonEmpty(detailText, "未配置");
            }
            string color = isActive ? BuildActiveHex : BuildInactiveHex;
            return BuildColoredText(label + "：", color, true)
                + "\n" + FormatBuildStageDetail(detailText, isActive, rarity);
        }

        private static string FormatBuildStageDetail(string value, bool isActive, ItemInstanceRarity rarity)
        {
            string text = NonEmpty(value, "未配置");
            if (!isActive)
            {
                return BuildColoredText(text, BuildInactiveHex, false);
            }

            return BuildColoredText(
                HighlightBuildCoreValue(text, RarityColorHex(rarity)),
                BuildActiveHex,
                false);
        }

        private static string HighlightBuildCoreValue(string value, string colorHex)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value ?? string.Empty;
            }

            StringBuilder builder = new(value.Length + 32);
            bool highlighted = false;
            for (int index = 0; index < value.Length;)
            {
                if (!IsBuildValueStart(value, index))
                {
                    builder.Append(value[index]);
                    index++;
                    continue;
                }

                int end = index + 1;
                while (end < value.Length && !IsBuildValueTerminator(value[end]))
                {
                    end++;
                }

                builder.Append(BuildColoredText(value.Substring(index, end - index), colorHex, true));
                highlighted = true;
                index = end;
            }

            return highlighted ? builder.ToString() : value;
        }

        private static bool IsBuildValueStart(string value, int index)
        {
            char current = value[index];
            if (current == '+' || current == '-' || current == '×')
            {
                return index + 1 < value.Length && char.IsDigit(value[index + 1]);
            }

            return char.IsDigit(current)
                && (index == 0 || char.IsWhiteSpace(value[index - 1]));
        }

        private static bool IsBuildValueTerminator(char value)
        {
            return char.IsWhiteSpace(value)
                || value == '；'
                || value == ';'
                || value == '，'
                || value == ','
                || value == '。'
                || value == ')'
                || value == '）';
        }

        private static string BuildColoredText(string value, string colorHex, bool bold)
        {
            string text = value ?? string.Empty;
            if (bold)
            {
                text = "<b>" + text + "</b>";
            }

            return ColorText(text, colorHex);
        }

        private static string FormatBuildStageName(ItemDetailBuildPreview preview)
        {
            int threshold = ResolveBuildStageThreshold(preview);
            return threshold > 0
                ? threshold.ToString(CultureInfo.InvariantCulture) + "件效果"
                : preview?.buildName ?? "构筑效果";
        }

        private static int ResolveBuildStageThreshold(ItemDetailBuildPreview preview)
        {
            if (preview == null)
            {
                return 0;
            }

            if (TryParseBuildProgress(preview.progressText, out _, out int max)
                && max > 0)
            {
                return max;
            }

            string name = preview.buildName ?? string.Empty;
            if (name.EndsWith("6", StringComparison.Ordinal)
                || name.Contains("Build6", StringComparison.Ordinal))
            {
                return 6;
            }

            if (name.EndsWith("4", StringComparison.Ordinal)
                || name.Contains("Build4", StringComparison.Ordinal))
            {
                return 4;
            }

            return 2;
        }

        private static int ResolveBuildCurrentCount(ItemDetailBuildPreview preview)
        {
            return preview != null && TryParseBuildProgress(preview.progressText, out int current, out _)
                ? current
                : 0;
        }

        private static bool TryParseBuildProgress(string progressText, out int current, out int max)
        {
            current = 0;
            max = 0;
            string[] parts = (progressText ?? string.Empty).Split('/');
            return parts.Length >= 2
                && int.TryParse(parts[0].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out current)
                && int.TryParse(parts[1].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out max);
        }

        private static string FormatBuildProgress(int currentCount, int maxPieceCount)
        {
            return currentCount.ToString(CultureInfo.InvariantCulture)
                + "/"
                + maxPieceCount.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatCandidateAffix(
            ItemCandidateAffixDefinition definition,
            ItemInstanceRarity rarity,
            long? overrideValueUnits = null,
            bool highlightKeyInfo = false)
        {
            ItemCandidateRarityValue range = definition?.rarityRanges?.FirstOrDefault(value => value != null
                && value.rarity == rarity);
            string rangeText = range == null ? string.Empty
                : FormatUnits(range.minUnits, definition.effectPayload?.valueUnitKey) + "—"
                    + FormatUnits(range.maxUnits, definition.effectPayload?.valueUnitKey);
            string keyInfoColorHex = highlightKeyInfo ? RarityColorHex(rarity) : string.Empty;
            string body = string.Equals(
                    definition?.affixId,
                    "signature_i010",
                    StringComparison.Ordinal)
                ? FormatWindAssistSignature(
                    definition.effectPayload,
                    overrideValueUnits,
                    keyInfoColorHex)
                : FormatEffectBody(
                    definition?.effectPayload,
                    overrideValueUnits,
                    keyInfoColorHex);
            return string.IsNullOrWhiteSpace(rangeText)
                ? body
                : body + "（当前品阶：" + rangeText + "）";
        }

        private static string FormatWindAssistSignature(
            ItemCandidateEffectPayload payload,
            long? overrideValueUnits,
            string keyInfoColorHex)
        {
            long value = overrideValueUnits ?? payload?.valueUnits ?? 0L;
            string nianCostText = FormatEffectKeyInfo(payload, value);
            if (!string.IsNullOrWhiteSpace(keyInfoColorHex))
            {
                nianCostText = ColorText(nianCostText, keyInfoColorHex);
            }

            string durationText = FormatRuntimeValue(
                Math.Max(0L, payload?.secondaryValueUnits ?? 0L),
                "turn");
            return "令旗点亮时，使相邻离火道具的持续时间延长"
                + durationText
                + "，并使下一次"
                + nianCostText;
        }

        private static string FormatCoreEffectBody(ItemBalanceCoreCandidate candidate, bool unlocked)
        {
            string body = FormatCoreEffectDetail(candidate);
            string unlockText = "Lv." + Math.Max(1, candidate?.unlockLevel ?? 1)
                + " / " + (candidate == null ? "当前品阶" : candidate.requiredRarity.ToDisplayName());
            return unlocked
                ? body
                : body + " " + GreyText("未解锁：" + unlockText);
        }

        private static string FormatCoreEffectDetail(ItemBalanceCoreCandidate candidate)
        {
            ItemCandidateEffectPayload payload = candidate?.effectPayload;
            if (payload == null || string.IsNullOrWhiteSpace(payload.effectId))
            {
                return "效果未配置";
            }

            string valueText = FormatRuntimeValue(payload.valueUnits, payload.valueUnitKey);
            string secondaryText = payload.secondaryValueUnits == 0
                ? string.Empty
                : "，副值" + FormatRuntimeValue(payload.secondaryValueUnits, payload.valueUnitKey);
            return CleanPlayerText(NonEmpty(payload.description, payload.displayName))
                + "，" + FormatTargetStat(payload.targetStatId)
                + FormatOperation(payload.operation)
                + FormatSignedValue(payload.operation, valueText)
                + secondaryText;
        }

        private static string FormatEffectBody(
            ItemCandidateEffectPayload payload,
            long? overrideValueUnits = null,
            string keyInfoColorHex = "")
        {
            if (payload == null || string.IsNullOrWhiteSpace(payload.effectId))
            {
                return "效果未配置";
            }

            long value = overrideValueUnits ?? payload.valueUnits;
            string description = CleanPlayerText(NonEmpty(payload.description, payload.displayName));
            string keyInfoPlain = FormatEffectKeyInfo(payload, value);
            string keyInfo = keyInfoPlain;
            if (!string.IsNullOrWhiteSpace(keyInfoColorHex))
            {
                keyInfo = ColorText(keyInfo, keyInfoColorHex);
            }

            if (ContainsTemplatePlaceholder(description))
            {
                return MergeTemplateDescription(
                    description,
                    keyInfo,
                    payload.secondaryValueUnits);
            }

            // A generic secondaryValueUnits has no secondary stat/unit schema. Showing it with the
            // primary unit produces false player text (for example "副值 1点"). The authored
            // description keeps the secondary effect semantics; signatures with an explicit display
            // mapping, such as I010, render their secondary value in that dedicated formatter.
            return description.TrimEnd('。', '；') + "；" + keyInfo;
        }

        private static string FormatEffectKeyInfo(ItemCandidateEffectPayload payload, long value)
        {
            string valueText = FormatRuntimeValue(value, payload?.valueUnitKey);
            return FormatTargetStat(payload?.targetStatId)
                + FormatOperation(payload?.operation ?? default)
                + FormatPlayerEffectValue(payload?.operation ?? default, valueText);
        }

        private static string FormatPlayerEffectValue(ItemCandidateEffectOperation operation, string valueText)
        {
            return operation switch
            {
                ItemCandidateEffectOperation.ReduceFlat => valueText,
                ItemCandidateEffectOperation.ReducePercent => valueText,
                ItemCandidateEffectOperation.Multiply => "×" + valueText,
                ItemCandidateEffectOperation.Convert => valueText,
                ItemCandidateEffectOperation.Override => valueText,
                _ => "+" + valueText
            };
        }

        private static bool ContainsTemplatePlaceholder(string value)
        {
            return !string.IsNullOrWhiteSpace(value)
                && (value.Contains("X", StringComparison.Ordinal)
                    || value.Contains("Y", StringComparison.Ordinal));
        }

        private static string MergeTemplateDescription(
            string description,
            string keyInfo,
            long secondaryValueUnits)
        {
            string text = description ?? string.Empty;
            if (text.Contains("X", StringComparison.Ordinal))
            {
                text = ReplaceXClause(text, keyInfo);
            }

            if (text.Contains("Y", StringComparison.Ordinal))
            {
                string secondaryText = secondaryValueUnits.ToString(CultureInfo.InvariantCulture);
                string secondaryPercentText = (secondaryValueUnits / 100d).ToString("0.##", CultureInfo.InvariantCulture) + "%";
                text = text.Replace("Y%", secondaryPercentText);
                text = text.Replace("Y层", secondaryText + "层");
                text = text.Replace("Y回合", secondaryText + "回合");
                text = text.Replace("Y次", secondaryText + "次");
                text = text.Replace("Y点", secondaryText + "点");
                text = text.Replace("Y", secondaryText);
            }

            return NormalizeChinesePunctuation(text);
        }

        private static string ReplaceXClause(string description, string replacement)
        {
            int xIndex = description.IndexOf('X');
            if (xIndex < 0)
            {
                return description;
            }

            int start = FindTemplateClauseStart(description, xIndex);
            int end = FindTemplateClauseEnd(description, xIndex);
            return description.Substring(0, start)
                + replacement
                + description.Substring(end);
        }

        private static int FindTemplateClauseStart(string value, int xIndex)
        {
            int punctuation = LastIndexOfAny(value, xIndex, '，', '。', '；', '、', '：', '\n');
            int condition = LastIndexOfAny(value, xIndex, '后', '时');
            return Math.Max(punctuation + 1, condition + 1);
        }

        private static int FindTemplateClauseEnd(string value, int xIndex)
        {
            int index = xIndex + 1;
            while (index < value.Length
                && value[index] != '，'
                && value[index] != '。'
                && value[index] != '；'
                && value[index] != '、'
                && value[index] != '\n')
            {
                index++;
            }

            return index;
        }

        private static int LastIndexOfAny(string value, int beforeIndex, params char[] tokens)
        {
            for (int index = Math.Min(beforeIndex - 1, value.Length - 1); index >= 0; index--)
            {
                if (tokens.Contains(value[index]))
                {
                    return index;
                }
            }

            return -1;
        }

        private static string NormalizeChinesePunctuation(string value)
        {
            string text = value ?? string.Empty;
            text = text.Replace("；。", "。");
            text = text.Replace("，。", "。");
            text = text.Replace("。。", "。");
            text = text.Replace("；；", "；");
            text = text.Replace("，，", "，");
            return text.Trim();
        }

        private static string ColorText(string value, string colorHex)
        {
            return string.IsNullOrWhiteSpace(colorHex)
                ? value ?? string.Empty
                : "<color=#" + colorHex.Trim().TrimStart('#') + ">" + (value ?? string.Empty) + "</color>";
        }

        private static string RarityColorHex(ItemInstanceRarity rarity)
        {
            return rarity switch
            {
                ItemInstanceRarity.White => "E3D8C3",
                ItemInstanceRarity.Green => "87B66A",
                ItemInstanceRarity.Blue => "68A9E6",
                ItemInstanceRarity.Purple => "B27DDF",
                ItemInstanceRarity.Orange => "E4A14B",
                _ => "D8CCB7"
            };
        }

        private static string GreyText(string value)
        {
            return "<color=#8A8A8A>" + (value ?? string.Empty) + "</color>";
        }

        private static string CleanPlayerText(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string text = value.Trim();
            text = text.Replace("本效果为Sandbox候选，不进入正式战斗结算。", string.Empty);
            text = text.Replace("Sandbox候选", string.Empty);
            text = text.Replace("候选内部冷却按载荷读取。", string.Empty);
            text = text.Replace("候选数据", string.Empty);
            text = text.Replace("候选", string.Empty);
            text = text.Replace("。。", "。");
            text = text.Replace("；。", "。");
            return text.Trim(' ', '\t', '\r', '\n', '；');
        }

        private static string FormatUnits(long value, string unit)
        {
            return unit switch
            {
                "basisPoint" => (value / 100d).ToString("0.##", CultureInfo.InvariantCulture) + "%",
                "point" => value.ToString(CultureInfo.InvariantCulture) + "点",
                "turn" => value.ToString(CultureInfo.InvariantCulture) + "回合",
                "count" => value.ToString(CultureInfo.InvariantCulture) + "次",
                "stack" => value.ToString(CultureInfo.InvariantCulture) + "层",
                "flat" => value.ToString(CultureInfo.InvariantCulture),
                _ => value.ToString(CultureInfo.InvariantCulture)
            };
        }

        private static ItemBalanceCandidateDetailResult Failure(
            ItemBalanceCandidateDetailRequestStatus status,
            string baseItemId,
            ItemInstanceRarity rarity,
            long rootSeed,
            params string[] errors)
        {
            string[] stableErrors = (errors ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
            string body = stableErrors.Length == 0 ? "Unknown validation error." : string.Join("\n", stableErrors);
            ItemDetailViewModel viewModel = new()
            {
                itemId = Normalize(baseItemId),
                baseItemId = Normalize(baseItemId),
                displayItemName = "实例预览不可用",
                displayRarityName = rarity.ToString(),
                rarityDisplayName = rarity.ToString(),
                displayPlayerSections = new List<ItemDetailSectionViewModel>
                {
                    Section("请求状态", "请求未生成实例；预览资产未被修改。")
                },
                displayDebugSections = new List<ItemDetailSectionViewModel>
                {
                    Section("调试 / 请求", "baseItemId: " + Normalize(baseItemId)
                        + "\nrarity: " + rarity
                        + "\nrootSeed: " + rootSeed.ToString(CultureInfo.InvariantCulture)),
                    Section("调试 / validationErrors", body),
                    Section("调试 / 边界", "BALANCE_CANDIDATE / EDITABLE / NOT_LIVE_LOCKED / NOT_BATTLE_CONNECTED")
                }
            };
            return new ItemBalanceCandidateDetailResult(
                status, baseItemId, rarity, rootSeed, null, null, viewModel, stableErrors, 0);
        }

        private static ItemDetailSectionViewModel Section(string title, string body, string stateKey = "")
        {
            string[] lines = (body ?? string.Empty).Split('\n')
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToArray();
            return new ItemDetailSectionViewModel(
                title,
                lines.Length == 0 ? "  当前无数据" : "  " + string.Join("\n  ", lines),
                stateKey,
                true);
        }

        private static ItemDetailSectionViewModel SectionOrHidden(string title, string body, string stateKey = "")
        {
            return IsEmptyPlayerBody(body)
                ? HiddenSection(stateKey)
                : Section(title, body, stateKey);
        }

        private static ItemDetailSectionViewModel HiddenSection(string stateKey)
        {
            return new ItemDetailSectionViewModel(string.Empty, string.Empty, stateKey, false);
        }

        private static bool IsEmptyPlayerBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return true;
            }

            string normalized = body.Replace("\r", string.Empty).Trim();
            return string.Equals(normalized, "当前无数据", StringComparison.Ordinal)
                || normalized.All(value => value == '─' || char.IsWhiteSpace(value));
        }

        private static string QualificationText(ItemBuildQualification qualification, bool faMen)
        {
            bool eligible = BuildQualificationAllows(qualification, faMen);
            return eligible
                ? "本实例具有该类 Build 资格；尚未计入或激活。"
                : qualification == ItemBuildQualification.None
                    ? "本实例没有 Build 资格。"
                    : "本实例不具有该类 Build 资格。";
        }

        private static bool BuildQualificationAllows(ItemBuildQualification qualification, bool faMen)
        {
            return faMen
                ? qualification == ItemBuildQualification.FaMenOnly || qualification == ItemBuildQualification.Dual
                : qualification == ItemBuildQualification.QiLeiOnly || qualification == ItemBuildQualification.Dual;
        }

        private static string FormatTextLines(IEnumerable<ItemDetailTextLine> values)
        {
            string[] lines = (values ?? Array.Empty<ItemDetailTextLine>())
                .Where(value => value != null)
                .Select(value => string.IsNullOrWhiteSpace(value.title)
                    ? value.body
                    : string.IsNullOrWhiteSpace(value.body)
                        ? value.title
                        : value.title + "：" + value.body)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
            return lines.Length == 0 ? "当前无实例词条" : string.Join("\n", lines);
        }

        private static string FormatCoreEffectLines(IEnumerable<ItemDetailTextLine> values, bool locked)
        {
            string[] lines = (values ?? Array.Empty<ItemDetailTextLine>())
                .Where(value => value != null)
                .Select(value =>
                {
                    string line = string.IsNullOrWhiteSpace(value.body)
                        ? value.title
                        : value.title + "：" + value.body;
                    return locked ? GreyText(line) : line;
                })
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();
            return lines.Length == 0 ? "当前无核心效果" : string.Join("\n", lines);
        }

        private static string Join(IReadOnlyList<string> values)
        {
            return values == null || values.Count == 0 ? "None" : string.Join(" | ", values);
        }

        private static string JoinLines(IEnumerable<string> values)
        {
            string[] lines = (values ?? Array.Empty<string>()).ToArray();
            return lines.Length == 0 ? "None" : string.Join(" | ", lines);
        }

        private static string NonEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
