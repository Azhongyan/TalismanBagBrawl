using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items;
using TalismanBag.Items.Balance;
using TalismanBag.Items.Generation;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;

namespace TalismanBag.Items.Canonical
{
    public static class CanonicalItemCatalogContract
    {
        public const string CatalogId = "canonical-item-catalog-31";
        public const string CatalogVersion = "playtest-v1";
        public const int OrdinaryItemCount = 30;
        public const int TotalItemCount = 31;
    }

    public sealed class CanonicalItemEffectDefinition
    {
        private readonly ReadOnlyCollection<string> effectTags;

        internal CanonicalItemEffectDefinition(ItemCandidateEffectPayload source)
        {
            effectId = source?.effectId ?? string.Empty;
            displayName = source?.displayName ?? string.Empty;
            description = source?.description ?? string.Empty;
            effectCategory = source?.effectCategory ?? ItemCandidateEffectCategory.NumericModifier;
            triggerEventId = source?.triggerEventId ?? string.Empty;
            conditionId = source?.conditionId ?? string.Empty;
            targetSelector = source?.targetSelector ?? string.Empty;
            targetStatId = source?.targetStatId ?? string.Empty;
            operation = source?.operation ?? ItemCandidateEffectOperation.AddFlat;
            valueUnitKey = source?.valueUnitKey ?? string.Empty;
            valueUnits = source?.valueUnits ?? 0L;
            secondaryValueUnits = source?.secondaryValueUnits ?? 0L;
            durationUnits = source?.durationUnits ?? 0L;
            stackLimit = source?.stackLimit ?? 1;
            internalCooldownUnits = source?.internalCooldownUnits ?? 0L;
            statusKey = source?.statusKey ?? string.Empty;
            statusFamilyKey = source?.statusFamilyKey ?? string.Empty;
            lifetimePolicy = source?.lifetimePolicy ?? "NONE";
            reapplyPolicy = source?.reapplyPolicy ?? "NONE";
            timeUnitKey = source?.timeUnitKey ?? string.Empty;
            firstTickDelayUnits = source?.firstTickDelayUnits ?? 0L;
            tickIntervalUnits = source?.tickIntervalUnits ?? 0L;
            tickSchedulePolicy = source?.tickSchedulePolicy ?? "NONE";
            tickDamageFormulaId = source?.tickDamageFormulaId ?? string.Empty;
            tickDamageValueUnits = source?.tickDamageValueUnits ?? 0L;
            tickDamageRatioBasisPoints =
                source?.tickDamageRatioBasisPoints ?? 0L;
            canCrit = source?.canCrit == true;
            countsAsHit = source?.countsAsHit == true;
            autoConsumeAtMaxStack = source?.autoConsumeAtMaxStack == true;
            consumeRule = source?.consumeRule ?? "NONE";
            expireRule = source?.expireRule ?? "NONE";
            cleanseRule = source?.cleanseRule ?? "NONE";
            parameterProfileId = source?.parameterProfileId ?? string.Empty;
            mutexGroupId = source?.mutexGroupId ?? string.Empty;
            effectTags = Array.AsReadOnly((source?.effectTags ?? new List<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }

        public string effectId { get; }
        public string displayName { get; }
        public string description { get; }
        public ItemCandidateEffectCategory effectCategory { get; }
        public string triggerEventId { get; }
        public string conditionId { get; }
        public string targetSelector { get; }
        public string targetStatId { get; }
        public ItemCandidateEffectOperation operation { get; }
        public string valueUnitKey { get; }
        public long valueUnits { get; }
        public long secondaryValueUnits { get; }
        public long durationUnits { get; }
        public int stackLimit { get; }
        public long internalCooldownUnits { get; }
        public string statusKey { get; }
        public string statusFamilyKey { get; }
        public string lifetimePolicy { get; }
        public string reapplyPolicy { get; }
        public string timeUnitKey { get; }
        public long firstTickDelayUnits { get; }
        public long tickIntervalUnits { get; }
        public string tickSchedulePolicy { get; }
        public string tickDamageFormulaId { get; }
        public long tickDamageValueUnits { get; }
        public long tickDamageRatioBasisPoints { get; }
        public bool canCrit { get; }
        public bool countsAsHit { get; }
        public bool autoConsumeAtMaxStack { get; }
        public string consumeRule { get; }
        public string expireRule { get; }
        public string cleanseRule { get; }
        public string parameterProfileId { get; }
        public string mutexGroupId { get; }
        public IReadOnlyList<string> EffectTags => effectTags;

        public bool hasStatusSemantics => statusKey.Length > 0
                                          || statusFamilyKey.Length > 0
                                          || lifetimePolicy != "NONE"
                                          || reapplyPolicy != "NONE"
                                          || timeUnitKey.Length > 0
                                          || firstTickDelayUnits > 0L
                                          || tickIntervalUnits > 0L
                                          || tickSchedulePolicy != "NONE"
                                          || tickDamageFormulaId.Length > 0
                                          || tickDamageValueUnits != 0L
                                          || tickDamageRatioBasisPoints != 0L
                                          || consumeRule != "NONE"
                                          || expireRule != "NONE"
                                          || cleanseRule != "NONE";

        public bool hasCompleteStatusSemantics => hasStatusSemantics
                                                  && statusKey.Length > 0
                                                  && statusFamilyKey.Length > 0
                                                  && lifetimePolicy != "NONE"
                                                  && reapplyPolicy != "NONE"
                                                  && stackLimit > 0
                                                  && consumeRule != "NONE"
                                                  && expireRule != "NONE"
                                                  && cleanseRule != "NONE"
                                                  && (lifetimePolicy != "TIMED"
                                                      || (timeUnitKey.Length > 0
                                                          && durationUnits > 0L))
                                                  && (tickDamageFormulaId.Length == 0
                                                      || (timeUnitKey.Length > 0
                                                          && firstTickDelayUnits > 0L
                                                          && tickIntervalUnits > 0L
                                                          && tickSchedulePolicy != "NONE"
                                                          && tickDamageRatioBasisPoints > 0L));

        internal bool isValid => effectId.Length > 0
                                 && triggerEventId.Length > 0
                                 && conditionId.Length > 0
                                 && targetSelector.Length > 0
                                 && targetStatId.Length > 0
                                 && valueUnitKey.Length > 0
                                 && (!hasStatusSemantics
                                     || hasCompleteStatusSemantics);
    }

    public sealed class CanonicalItemStatRange
    {
        internal CanonicalItemStatRange(ItemBalanceRange source)
        {
            statId = source?.statId ?? string.Empty;
            minUnits = source?.minUnits ?? 0L;
            maxUnits = source?.maxUnits ?? 0L;
        }

        public string statId { get; }
        public long minUnits { get; }
        public long maxUnits { get; }
    }

    public sealed class CanonicalItemCoreDefinition
    {
        internal CanonicalItemCoreDefinition(ItemBalanceCoreCandidate source)
        {
            coreEffectId = source?.coreEffectId ?? string.Empty;
            displayName = source?.displayName ?? string.Empty;
            description = source?.description ?? string.Empty;
            isUltimate = source?.isUltimate == true;
            nodeKind = source?.nodeKind ?? string.Empty;
            unlockLevel = source?.unlockLevel ?? 0;
            requiredRarity = source?.requiredRarity ?? ItemInstanceRarity.White;
            effect = new CanonicalItemEffectDefinition(source?.effectPayload);
        }

        public string coreEffectId { get; }
        public string displayName { get; }
        public string description { get; }
        public bool isUltimate { get; }
        public string nodeKind { get; }
        public int unlockLevel { get; }
        public ItemInstanceRarity requiredRarity { get; }
        public CanonicalItemEffectDefinition effect { get; }
    }

    public sealed class CanonicalItemPresentationDefinition
    {
        internal CanonicalItemPresentationDefinition(
            ItemInnerDataDefinition identity,
            ItemCandidateDisplayProfile source)
        {
            iconKey = !string.IsNullOrWhiteSpace(source?.iconKey)
                ? source.iconKey
                : identity?.iconPlaceholderKey ?? string.Empty;
            triggerDescription = source?.triggerDescription ?? identity?.triggerText ?? string.Empty;
            basicEffectDescription = source?.basicEffectDescription
                                     ?? identity?.basicEffectText
                                     ?? string.Empty;
            lightingDescription = source?.lightingDescription ?? string.Empty;
            placementRecommendation = source?.placementRecommendation
                                      ?? identity?.placementHint
                                      ?? string.Empty;
            flavorText = source?.flavorText ?? identity?.flavorText ?? string.Empty;
            presentationState = source?.presentationState
                                ?? ItemPresentationConfirmationState.Unconfirmed;
            effectFamilyKey = source?.effectFamilyKey ?? string.Empty;
            presentationStyleKey = source?.presentationStyleKey ?? string.Empty;
            cueIdentity = source?.cueIdentity ?? string.Empty;
        }

        public string iconKey { get; }
        public string triggerDescription { get; }
        public string basicEffectDescription { get; }
        public string lightingDescription { get; }
        public string placementRecommendation { get; }
        public string flavorText { get; }
        public ItemPresentationConfirmationState presentationState { get; }
        public string effectFamilyKey { get; }
        public string presentationStyleKey { get; }
        public string cueIdentity { get; }
    }

    public sealed class CanonicalItemRarityProfile
    {
        private readonly ReadOnlyCollection<CanonicalItemStatRange> statRanges;
        private readonly ReadOnlyCollection<string> eligibleCoreEffectIds;
        private readonly ReadOnlyCollection<string> visibleCoreEffectIds;

        internal CanonicalItemRarityProfile(ItemBalanceRarityVersion source)
        {
            rarity = source.rarity;
            versionKey = source.versionKey ?? string.Empty;
            cultivationPotentialProfileId = source.cultivationPotentialProfileId ?? string.Empty;
            candidateItemPower = source.candidateItemPower;
            statRanges = Array.AsReadOnly((source.statRanges ?? new List<ItemBalanceRange>())
                .Where(value => value != null)
                .Select(value => new CanonicalItemStatRange(value))
                .OrderBy(value => value.statId, StringComparer.Ordinal)
                .ToArray());
            eligibleCoreEffectIds = Array.AsReadOnly((source.eligibleCoreEffectIds ?? new List<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            visibleCoreEffectIds = Array.AsReadOnly((source.visibleCoreEffectIds ?? new List<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }

        public ItemInstanceRarity rarity { get; }
        public string versionKey { get; }
        public string cultivationPotentialProfileId { get; }
        public long candidateItemPower { get; }
        public IReadOnlyList<CanonicalItemStatRange> StatRanges => statRanges;
        public IReadOnlyList<string> EligibleCoreEffectIds => eligibleCoreEffectIds;
        public IReadOnlyList<string> VisibleCoreEffectIds => visibleCoreEffectIds;

        public CanonicalItemStatRange GetStatRange(string statId) => statRanges.FirstOrDefault(
            value => string.Equals(value.statId, statId, StringComparison.Ordinal));
    }

    public sealed class CanonicalItemDefinition
    {
        private readonly ReadOnlyCollection<Vector2Int> shapeCells;
        private readonly IReadOnlyDictionary<ItemInstanceRarity, CanonicalItemRarityProfile> rarityProfiles;
        private readonly ReadOnlyCollection<string> randomAffixIds;
        private readonly ReadOnlyCollection<CanonicalItemCoreDefinition> coreDefinitions;
        private readonly IReadOnlyDictionary<string, CanonicalItemEffectDefinition>
            generatedAffixEffectsById;

        internal CanonicalItemDefinition(
            ItemInnerDataDefinition identity,
            ItemBalanceProfile balanceProfile,
            ItemBalanceWorkbenchCatalog sourceCatalog)
        {
            baseItemId = identity?.itemId ?? string.Empty;
            displayName = !string.IsNullOrWhiteSpace(balanceProfile?.displayName)
                ? balanceProfile.displayName
                : identity?.displayName ?? string.Empty;
            itemFamily = identity?.itemFamily ?? ItemFamilyTag.ZhonggongSource;
            faMenKey = identity?.FaMenKey ?? string.Empty;
            qiLeiKey = identity?.QiLeiKey ?? string.Empty;
            shapeId = identity?.shapeId ?? string.Empty;
            shapeCells = Array.AsReadOnly((identity?.ShapeCells ?? Array.Empty<Vector2Int>()).ToArray());
            coreCellLocal = identity?.coreCellLocal ?? Vector2Int.zero;
            isLightingSource = identity?.isLightingSource == true;
            primaryStatId = balanceProfile?.primaryStatId ?? string.Empty;
            secondaryStatId = balanceProfile?.secondaryStatId ?? string.Empty;
            fixedAffixId = balanceProfile?.fixedAffixId ?? string.Empty;
            randomPoolId = balanceProfile?.randomPoolId ?? string.Empty;
            combatEffect = new CanonicalItemEffectDefinition(balanceProfile?.signatureAffix?.effectPayload);
            presentation = new CanonicalItemPresentationDefinition(identity, balanceProfile?.candidateDisplay);
            randomAffixIds = Array.AsReadOnly((balanceProfile?.randomAffixes
                                              ?? new List<ItemBalanceWeightedAffix>())
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.affixId))
                .Select(value => value.affixId)
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
            generatedAffixEffectsById = new[] { fixedAffixId }
                .Concat(randomAffixIds)
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .Select(value => new
                {
                    AffixId = value,
                    Effect = new CanonicalItemEffectDefinition(
                        sourceCatalog?.FindCandidateAffix(value)?.effectPayload)
                })
                .Where(value => value.Effect.isValid)
                .ToDictionary(
                    value => value.AffixId,
                    value => value.Effect,
                    StringComparer.Ordinal);
            coreDefinitions = Array.AsReadOnly((balanceProfile?.coreCandidates
                                                ?? new List<ItemBalanceCoreCandidate>())
                .Where(value => value != null)
                .Select(value => new CanonicalItemCoreDefinition(value))
                .OrderBy(value => value.unlockLevel)
                .ThenBy(value => value.coreEffectId, StringComparer.Ordinal)
                .ToArray());
            rarityProfiles = (balanceProfile?.rarityVersions ?? new List<ItemBalanceRarityVersion>())
                .Where(value => value != null)
                .GroupBy(value => value.rarity)
                .ToDictionary(group => group.Key,
                    group => new CanonicalItemRarityProfile(group.First()));
            isOrdinaryDropEligible = identity != null
                && identity.itemFamily == ItemFamilyTag.FaMenCombat
                && !identity.isLightingSource
                && balanceProfile != null;
            requiresCleanseablePlayerStatus =
                string.Equals(primaryStatId, "cleanse", StringComparison.Ordinal)
                || string.Equals(
                    combatEffect.triggerEventId,
                    "on_cleanse",
                    StringComparison.Ordinal)
                || string.Equals(
                    combatEffect.conditionId,
                    "target_has_debuff",
                    StringComparison.Ordinal)
                || string.Equals(
                    combatEffect.targetStatId,
                    "cleanse",
                    StringComparison.Ordinal)
                || string.Equals(
                    combatEffect.targetStatId,
                    "debuff_to_heal",
                    StringComparison.Ordinal);
        }

        public string baseItemId { get; }
        public string displayName { get; }
        public ItemFamilyTag itemFamily { get; }
        public string faMenKey { get; }
        public string qiLeiKey { get; }
        public string shapeId { get; }
        public IReadOnlyList<Vector2Int> ShapeCells => shapeCells;
        public Vector2Int coreCellLocal { get; }
        public bool isLightingSource { get; }
        public bool isOrdinaryDropEligible { get; }
        public bool requiresCleanseablePlayerStatus { get; }
        public string primaryStatId { get; }
        public string secondaryStatId { get; }
        public string fixedAffixId { get; }
        public string randomPoolId { get; }
        public CanonicalItemEffectDefinition combatEffect { get; }
        public CanonicalItemPresentationDefinition presentation { get; }
        public IReadOnlyList<string> RandomAffixIds => randomAffixIds;
        public IReadOnlyList<CanonicalItemCoreDefinition> CoreDefinitions => coreDefinitions;

        public CanonicalItemEffectDefinition GetGeneratedAffixEffect(
            string affixId)
        {
            return !string.IsNullOrWhiteSpace(affixId)
                   && generatedAffixEffectsById.TryGetValue(
                       affixId.Trim(),
                       out CanonicalItemEffectDefinition effect)
                ? effect
                : null;
        }

        internal CanonicalItemRarityProfile GetRarityProfile(ItemInstanceRarity rarity) =>
            rarityProfiles.TryGetValue(rarity, out CanonicalItemRarityProfile profile) ? profile : null;
    }

    internal sealed class CanonicalItemCatalogSnapshot
    {
        private readonly ReadOnlyCollection<CanonicalItemDefinition> definitions;
        private readonly ReadOnlyCollection<ItemInnerDataDefinition>
            itemSystemCatalogProjection;
        private readonly IReadOnlyDictionary<string, CanonicalItemDefinition> byBaseItemId;

        internal CanonicalItemCatalogSnapshot(
            ItemBalanceCompiledData compiledData,
            IEnumerable<ItemInnerDataDefinition> identities,
            IEnumerable<CanonicalItemDefinition> definitions)
        {
            CompiledData = compiledData;
            this.definitions = Array.AsReadOnly((definitions ?? Array.Empty<CanonicalItemDefinition>())
                .Where(value => value != null)
                .OrderBy(value => value.baseItemId, StringComparer.Ordinal)
                .ToArray());
            itemSystemCatalogProjection = Array.AsReadOnly((identities
                    ?? Array.Empty<ItemInnerDataDefinition>())
                .Where(value => value != null)
                .Select(ItemSystemSnapshotInput.CloneCatalogItem)
                .OrderBy(value => value.itemId, StringComparer.Ordinal)
                .ToArray());
            byBaseItemId = this.definitions.ToDictionary(value => value.baseItemId, StringComparer.Ordinal);
        }

        internal ItemBalanceCompiledData CompiledData { get; }
        internal IReadOnlyList<CanonicalItemDefinition> Definitions => definitions;
        internal IReadOnlyList<ItemInnerDataDefinition>
            ItemSystemCatalogProjection => itemSystemCatalogProjection;
        internal IEnumerable<CanonicalItemDefinition> OrdinaryDropDefinitions =>
            definitions.Where(value => value.isOrdinaryDropEligible);

        internal CanonicalItemDefinition GetDefinition(string baseItemId)
        {
            return !string.IsNullOrWhiteSpace(baseItemId)
                && byBaseItemId.TryGetValue(baseItemId.Trim(), out CanonicalItemDefinition definition)
                    ? definition
                    : null;
        }
    }

    internal sealed class CanonicalItemCatalogCompileResult
    {
        internal CanonicalItemCatalogCompileResult(
            CanonicalItemCatalogSnapshot snapshot,
            IEnumerable<string> errors)
        {
            Snapshot = snapshot;
            Errors = Array.AsReadOnly((errors ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray());
        }

        internal bool isSuccess => Snapshot != null && Errors.Count == 0;
        internal CanonicalItemCatalogSnapshot Snapshot { get; }
        internal IReadOnlyList<string> Errors { get; }
    }

    internal static class CanonicalItemCatalogCompiler
    {
        internal static CanonicalItemCatalogCompileResult Compile(ItemBalanceWorkbenchCatalog sourceCatalog)
        {
            List<string> errors = new();
            if (sourceCatalog == null)
            {
                errors.Add("CANONICAL_CATALOG_SOURCE_MISSING");
                return new CanonicalItemCatalogCompileResult(null, errors);
            }

            ItemInnerDataDefinition[] identities = ItemInnerDataCatalog.AllItems
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.itemId))
                .OrderBy(value => value.itemId, StringComparer.Ordinal)
                .ToArray();
            if (identities.Length != 31)
            {
                errors.Add($"CANONICAL_IDENTITY_COUNT_INVALID:{identities.Length}");
            }

            foreach (IGrouping<string, ItemInnerDataDefinition> duplicate in identities
                         .GroupBy(value => value.itemId, StringComparer.Ordinal)
                         .Where(group => group.Count() > 1))
            {
                errors.Add("CANONICAL_IDENTITY_DUPLICATE:" + duplicate.Key);
            }

            Dictionary<string, ItemBalanceProfile> profiles = sourceCatalog.profiles
                .Where(value => value != null && !string.IsNullOrWhiteSpace(value.baseItemId))
                .GroupBy(value => value.baseItemId, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            List<CanonicalItemDefinition> definitions = new();
            foreach (ItemInnerDataDefinition identity in identities)
            {
                bool ordinary = identity.itemFamily == ItemFamilyTag.FaMenCombat && !identity.isLightingSource;
                profiles.TryGetValue(identity.itemId, out ItemBalanceProfile profile);
                if (ordinary && profile == null)
                {
                    errors.Add("CANONICAL_BALANCE_PROFILE_MISSING:" + identity.itemId);
                }
                else if (!ordinary && profile != null)
                {
                    errors.Add("CANONICAL_SPECIAL_ITEM_HAS_ORDINARY_PROFILE:" + identity.itemId);
                }

                if (profile != null)
                {
                    if (!string.Equals(profile.faMenTag, identity.FaMenKey, StringComparison.Ordinal))
                    {
                        errors.Add("CANONICAL_FAMEN_TAG_MISMATCH:" + identity.itemId);
                    }

                    if (!string.Equals(profile.qiLeiTag, identity.QiLeiKey, StringComparison.Ordinal))
                    {
                        errors.Add("CANONICAL_QILEI_TAG_MISMATCH:" + identity.itemId);
                    }

                    foreach (ItemInstanceRarityDefinition rarity in ItemInstanceRarityCatalog.All)
                    {
                        if (profile.FindVersion(rarity.rarity) == null)
                        {
                            errors.Add($"CANONICAL_RARITY_PROFILE_MISSING:{identity.itemId}@{rarity.stableKey}");
                        }
                    }

                    CanonicalItemEffectDefinition combatEffect =
                        new CanonicalItemEffectDefinition(profile.signatureAffix?.effectPayload);
                    if (!combatEffect.isValid)
                    {
                        errors.Add("CANONICAL_COMBAT_EFFECT_MISSING:" + identity.itemId);
                    }
                }

                definitions.Add(new CanonicalItemDefinition(
                    identity,
                    profile,
                    sourceCatalog));
            }

            HashSet<string> knownIds = identities.Select(value => value.itemId).ToHashSet(StringComparer.Ordinal);
            foreach (string extraProfileId in profiles.Keys.Where(value => !knownIds.Contains(value)))
            {
                errors.Add("CANONICAL_BALANCE_PROFILE_UNKNOWN:" + extraProfileId);
            }

            if (definitions.Count(value => value.isOrdinaryDropEligible) != 30)
            {
                errors.Add("CANONICAL_ORDINARY_DROP_COUNT_INVALID");
            }

            ItemBalanceCompiledData compiled = null;
            if (errors.Count == 0)
            {
                try
                {
                    compiled = ItemBalanceWorkbenchCompiler.Compile(sourceCatalog);
                    if (!compiled.Foundation.isValid
                        || !compiled.StatSchema.isValid
                        || !compiled.AffixSchema.isValid
                        || !compiled.CoreBuildSchema.isValid)
                    {
                        errors.Add("CANONICAL_COMPILED_SCHEMA_INVALID");
                    }
                }
                catch (Exception exception)
                {
                    errors.Add("CANONICAL_COMPILE_FAILED:" + exception.GetType().Name);
                }
            }

            CanonicalItemCatalogSnapshot snapshot = errors.Count == 0
                ? new CanonicalItemCatalogSnapshot(
                    compiled,
                    identities,
                    definitions)
                : null;
            return new CanonicalItemCatalogCompileResult(snapshot, errors);
        }
    }

    public sealed class CanonicalItemDefinitionResolver
    {
        private readonly CanonicalItemCatalogSnapshot snapshot;

        private CanonicalItemDefinitionResolver(CanonicalItemCatalogSnapshot snapshot)
        {
            this.snapshot = snapshot;
        }

        internal CanonicalItemCatalogSnapshot Snapshot => snapshot;

        public IReadOnlyList<CanonicalItemDefinition> OrdinaryDropDefinitions =>
            snapshot?.OrdinaryDropDefinitions.ToArray()
            ?? Array.Empty<CanonicalItemDefinition>();

        public static bool TryCreate(
            ItemBalanceWorkbenchCatalog sourceCatalog,
            out CanonicalItemDefinitionResolver resolver,
            out IReadOnlyList<string> errors)
        {
            CanonicalItemCatalogCompileResult result = CanonicalItemCatalogCompiler.Compile(sourceCatalog);
            resolver = result.isSuccess ? new CanonicalItemDefinitionResolver(result.Snapshot) : null;
            errors = result.Errors;
            return resolver != null;
        }

        public CanonicalItemDefinition GetDefinition(string baseItemId) => snapshot.GetDefinition(baseItemId);

        public CanonicalItemRarityProfile GetRarityProfile(string baseItemId, ItemInstanceRarity rarity) =>
            GetDefinition(baseItemId)?.GetRarityProfile(rarity);

        public IReadOnlyList<ItemInnerDataDefinition>
            CreateItemSystemCatalogProjection() =>
                snapshot.ItemSystemCatalogProjection
                    .Select(ItemSystemSnapshotInput.CloneCatalogItem)
                    .ToArray();

        public IReadOnlyList<string> Validate() => snapshot == null
            ? Array.AsReadOnly(new[] { "CANONICAL_CATALOG_NOT_INITIALIZED" })
            : Array.Empty<string>();
    }

    public static class CanonicalItemPresentationIdentityResolver
    {
        public static bool TryResolve(
            CanonicalItemDefinition definition,
            out string effectFamilyKey,
            out string presentationStyleKey,
            out string cueIdentity)
        {
            effectFamilyKey = string.Empty;
            presentationStyleKey = string.Empty;
            cueIdentity = string.Empty;
            if (definition == null
                || !definition.isOrdinaryDropEligible
                || definition.presentation == null
                || definition.presentation.presentationState == ItemPresentationConfirmationState.Unconfirmed
                || string.IsNullOrWhiteSpace(definition.presentation.effectFamilyKey)
                || string.IsNullOrWhiteSpace(definition.presentation.presentationStyleKey)
                || string.IsNullOrWhiteSpace(definition.presentation.cueIdentity))
                return false;

            effectFamilyKey = definition.presentation.effectFamilyKey;
            presentationStyleKey = definition.presentation.presentationStyleKey;
            cueIdentity = definition.presentation.cueIdentity;
            return true;
        }
    }

    public static class CanonicalItemArtworkResolver
    {
        private const string ResourcePrefix = "item_daoju/";

        public static bool TryResolve(
            CanonicalItemDefinition definition,
            ItemInstanceRarity rarity,
            out Sprite artwork,
            out string resourcePath)
        {
            artwork = null;
            resourcePath = string.Empty;
            if (definition == null
                || !definition.isOrdinaryDropEligible
                || string.IsNullOrWhiteSpace(definition.baseItemId))
                return false;

            string folder = definition.faMenKey switch
            {
                "zhenlei" => "震雷法",
                "lihuo" => "离火法",
                "zhongyue" => "中岳法",
                "xuanshui" => "玄水法",
                "taibai" => "太白法",
                _ => string.Empty
            };
            int rarityIndex = rarity.ToTierIndex() + 1;
            if (folder.Length == 0 || rarityIndex < 1 || rarityIndex > 5)
                return false;

            resourcePath = ResourcePrefix + folder + "/"
                           + definition.baseItemId + "/"
                           + definition.baseItemId + "_" + rarityIndex;
            artwork = Resources.Load<Sprite>(resourcePath);
            return artwork != null;
        }
    }
}
