using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items.Build;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.DropCore;
using TalismanBag.Items.Generation.Rolling;

namespace TalismanBag.Items.Canonical
{
    public static class CanonicalItemBattleCapabilities
    {
        public const string PlayerCleanseableStatus =
            "player.status.cleanseable";
    }

    public sealed class CanonicalItemRarityDropRule
    {
        public CanonicalItemRarityDropRule(
            ItemInstanceRarity rarity,
            double baseWeight,
            double stageMultiplier)
        {
            this.rarity = rarity;
            this.baseWeight = baseWeight;
            this.stageMultiplier = stageMultiplier;
        }

        public ItemInstanceRarity rarity { get; }
        public double baseWeight { get; }
        public double stageMultiplier { get; }

        public double WeightAtStage(int stageIndex) =>
            stageIndex < 1 ? 0d : baseWeight * Math.Pow(stageMultiplier, stageIndex - 1);
    }

    public sealed class CanonicalItemDropPolicyProfile
    {
        private readonly ReadOnlyCollection<CanonicalItemRarityDropRule> rarityRules;

        private CanonicalItemDropPolicyProfile(
            string policyId,
            IEnumerable<CanonicalItemRarityDropRule> rarityRules,
            double duplicateDecay,
            double nearestBuildAdvanceMultiplier)
        {
            this.policyId = policyId;
            this.rarityRules = Array.AsReadOnly((rarityRules ?? Array.Empty<CanonicalItemRarityDropRule>())
                .OrderBy(value => value.rarity.ToTierIndex())
                .ToArray());
            this.duplicateDecay = duplicateDecay;
            this.nearestBuildAdvanceMultiplier = nearestBuildAdvanceMultiplier;
        }

        public string policyId { get; }
        public IReadOnlyList<CanonicalItemRarityDropRule> RarityRules => rarityRules;
        public double duplicateDecay { get; }
        public double nearestBuildAdvanceMultiplier { get; }

        public static CanonicalItemDropPolicyProfile PlaytestV1 { get; } = new(
            "canonical-item-drop-playtest-v1",
            new[]
            {
                new CanonicalItemRarityDropRule(ItemInstanceRarity.White, 70d, 0.78d),
                new CanonicalItemRarityDropRule(ItemInstanceRarity.Green, 24d, 1d),
                new CanonicalItemRarityDropRule(ItemInstanceRarity.Blue, 5d, 1.45d),
                new CanonicalItemRarityDropRule(ItemInstanceRarity.Purple, 0.9d, 1.8d),
                new CanonicalItemRarityDropRule(ItemInstanceRarity.Orange, 0.1d, 2.2d)
            },
            0.55d,
            1.35d);
    }

    public sealed class CanonicalItemDropRequest
    {
        private readonly IReadOnlyDictionary<string, int> currentRunCopies;
        private readonly ReadOnlyCollection<string> availableBattleCapabilities;

        public CanonicalItemDropRequest(
            string requestId,
            string itemInstanceId,
            string sourceContextId,
            string sourceKey,
            int sourceOrdinal,
            int stageIndex,
            long rootSeed,
            IReadOnlyDictionary<string, int> currentRunCopies,
            ItemBuildSynergyResolutionResult buildSnapshot,
            IEnumerable<string> availableBattleCapabilities = null)
        {
            this.requestId = Normalize(requestId);
            this.itemInstanceId = Normalize(itemInstanceId);
            this.sourceContextId = Normalize(sourceContextId);
            this.sourceKey = Normalize(sourceKey);
            this.sourceOrdinal = sourceOrdinal;
            this.stageIndex = stageIndex;
            this.rootSeed = rootSeed;
            this.currentRunCopies = new ReadOnlyDictionary<string, int>(
                (currentRunCopies ?? new Dictionary<string, int>())
                .ToDictionary(value => value.Key, value => value.Value, StringComparer.Ordinal));
            this.buildSnapshot = buildSnapshot;
            this.availableBattleCapabilities = Array.AsReadOnly(
                (availableBattleCapabilities ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray());
        }

        public string requestId { get; }
        public string itemInstanceId { get; }
        public string sourceContextId { get; }
        public string sourceKey { get; }
        public int sourceOrdinal { get; }
        public int stageIndex { get; }
        public long rootSeed { get; }
        public IReadOnlyDictionary<string, int> CurrentRunCopies => currentRunCopies;
        public ItemBuildSynergyResolutionResult buildSnapshot { get; }
        public IReadOnlyList<string> AvailableBattleCapabilities =>
            availableBattleCapabilities;

        private static string Normalize(string value) =>
            string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    public sealed class CanonicalItemDropResult
    {
        internal CanonicalItemDropResult(
            CanonicalItemDefinition definition,
            ItemInstanceRarity rarity,
            ItemInstanceRollResult itemRoll,
            IEnumerable<string> errors)
        {
            Definition = definition;
            this.rarity = rarity;
            ItemRoll = itemRoll;
            Errors = Array.AsReadOnly((errors ?? Array.Empty<string>()).ToArray());
        }

        public bool isSuccess => Definition != null && ItemRoll?.isSuccess == true && Errors.Count == 0;
        public CanonicalItemDefinition Definition { get; }
        public ItemInstanceRarity rarity { get; }
        public ItemInstanceRollResult ItemRoll { get; }
        public IReadOnlyList<string> Errors { get; }
    }

    public static class CanonicalItemDropPolicy
    {
        public static CanonicalItemDropResult Roll(
            CanonicalItemDefinitionResolver resolver,
            CanonicalItemDropRequest request,
            CanonicalItemDropPolicyProfile profile = null)
        {
            profile ??= CanonicalItemDropPolicyProfile.PlaytestV1;
            List<string> errors = Validate(resolver, request, profile);
            if (errors.Count > 0)
            {
                return new CanonicalItemDropResult(null, default, null, errors);
            }

            if (!TryCreateStream(request, profile.policyId, "rarity", out DeterministicItemRandom.Stream rarityStream))
            {
                return Failure("CANONICAL_DROP_RARITY_STREAM_INVALID");
            }

            WeightedValue<ItemInstanceRarity>[] rarityWeights = profile.RarityRules
                .Select(value => new WeightedValue<ItemInstanceRarity>(
                    value.rarity,
                    value.WeightAtStage(request.stageIndex)))
                .ToArray();
            if (!TrySelect(rarityStream, rarityWeights, out ItemInstanceRarity rarity))
            {
                return Failure("CANONICAL_DROP_RARITY_WEIGHT_INVALID");
            }

            CanonicalItemDefinition[] candidates = resolver.Snapshot.OrdinaryDropDefinitions
                .Where(value => value.GetRarityProfile(rarity) != null)
                .Where(value => IsBattleCapabilityCompatible(value, request))
                .OrderBy(value => value.baseItemId, StringComparer.Ordinal)
                .ToArray();
            int nearestBuildGap = FindNearestBuildGap(request.buildSnapshot);
            WeightedValue<CanonicalItemDefinition>[] candidateWeights = candidates
                .Select(value => new WeightedValue<CanonicalItemDefinition>(
                    value,
                    ItemWeight(value, request, profile, nearestBuildGap)))
                .ToArray();

            if (!TryCreateStream(request, profile.policyId, "item", out DeterministicItemRandom.Stream itemStream)
                || !TrySelect(itemStream, candidateWeights, out CanonicalItemDefinition definition))
            {
                return Failure("CANONICAL_DROP_ITEM_WEIGHT_INVALID");
            }

            ItemInstanceRollResult itemRoll = CanonicalItemInstanceFactory.Create(
                resolver,
                request.itemInstanceId,
                definition.baseItemId,
                rarity,
                request.rootSeed);
            if (!itemRoll.isSuccess)
            {
                return new CanonicalItemDropResult(definition, rarity, itemRoll,
                    itemRoll.ValidationErrors.Select(value => value.code));
            }

            return new CanonicalItemDropResult(definition, rarity, itemRoll, Array.Empty<string>());

            CanonicalItemDropResult Failure(string code) =>
                new(null, default, null, new[] { code });
        }

        public static bool IsBattleCapabilityCompatible(
            CanonicalItemDefinition definition,
            CanonicalItemDropRequest request)
        {
            if (definition == null || request == null)
            {
                return false;
            }

            return !definition.requiresCleanseablePlayerStatus
                || request.AvailableBattleCapabilities.Contains(
                    CanonicalItemBattleCapabilities.PlayerCleanseableStatus,
                    StringComparer.Ordinal);
        }

        private static List<string> Validate(
            CanonicalItemDefinitionResolver resolver,
            CanonicalItemDropRequest request,
            CanonicalItemDropPolicyProfile profile)
        {
            List<string> errors = new();
            if (resolver?.Snapshot == null) errors.Add("CANONICAL_DROP_RESOLVER_MISSING");
            if (request == null) return new List<string> { "CANONICAL_DROP_REQUEST_MISSING" };
            if (string.IsNullOrWhiteSpace(request.requestId)
                || string.IsNullOrWhiteSpace(request.itemInstanceId)
                || string.IsNullOrWhiteSpace(request.sourceContextId)
                || string.IsNullOrWhiteSpace(request.sourceKey)
                || request.sourceOrdinal < 1
                || request.stageIndex < 1)
            {
                errors.Add("CANONICAL_DROP_REQUEST_INVALID");
            }

            if (profile == null || profile.RarityRules.Count != ItemInstanceRarityCatalog.All.Count)
            {
                errors.Add("CANONICAL_DROP_PROFILE_INVALID");
            }

            return errors;
        }

        private static bool TryCreateStream(
            CanonicalItemDropRequest request,
            string policyId,
            string domain,
            out DeterministicItemRandom.Stream stream)
        {
            return ItemDropDeterministicDomain.TryCreateStream(
                request.rootSeed,
                ItemDropDeterministicDomain.SupportedGenerationVersion,
                request.requestId,
                request.sourceContextId,
                request.sourceKey,
                request.sourceOrdinal,
                new[] { policyId, domain, "stage-" + request.stageIndex },
                out stream);
        }

        private static double ItemWeight(
            CanonicalItemDefinition definition,
            CanonicalItemDropRequest request,
            CanonicalItemDropPolicyProfile profile,
            int nearestBuildGap)
        {
            request.CurrentRunCopies.TryGetValue(definition.baseItemId, out int copies);
            double duplicateWeight = Math.Pow(profile.duplicateDecay, Math.Max(0, copies));
            double buildWeight = AdvancesNearestBuild(definition, request.buildSnapshot, nearestBuildGap)
                ? profile.nearestBuildAdvanceMultiplier
                : 1d;
            return duplicateWeight * buildWeight;
        }

        private static int FindNearestBuildGap(ItemBuildSynergyResolutionResult buildSnapshot)
        {
            return (buildSnapshot?.FaMenBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Concat(buildSnapshot?.QiLeiBuilds ?? Array.Empty<ItemBuildTrackResult>())
                .Where(value => value != null && value.nextStagePieceCount > value.litItemCount)
                .Select(value => value.nextStagePieceCount - value.litItemCount)
                .DefaultIfEmpty(0)
                .Min();
        }

        private static bool AdvancesNearestBuild(
            CanonicalItemDefinition definition,
            ItemBuildSynergyResolutionResult buildSnapshot,
            int nearestBuildGap)
        {
            if (definition == null || buildSnapshot == null || nearestBuildGap <= 0)
            {
                return false;
            }

            return MatchesNearest(definition.faMenKey, buildSnapshot.FaMenBuilds, nearestBuildGap)
                || MatchesNearest(definition.qiLeiKey, buildSnapshot.QiLeiBuilds, nearestBuildGap);
        }

        private static bool MatchesNearest(
            string stableTag,
            IEnumerable<ItemBuildTrackResult> tracks,
            int nearestBuildGap)
        {
            return tracks.Any(value => value != null
                && string.Equals(value.stableTag, stableTag, StringComparison.Ordinal)
                && value.nextStagePieceCount - value.litItemCount == nearestBuildGap);
        }

        private static bool TrySelect<T>(
            DeterministicItemRandom.Stream stream,
            IReadOnlyList<WeightedValue<T>> values,
            out T selected)
        {
            selected = default;
            double total = values?.Where(value => value.weight > 0d).Sum(value => value.weight) ?? 0d;
            if (stream == null || total <= 0d)
            {
                return false;
            }

            double unit = (stream.NextUInt64() >> 11) * (1d / 9007199254740992d);
            double cursor = unit * total;
            foreach (WeightedValue<T> value in values.Where(value => value.weight > 0d))
            {
                cursor -= value.weight;
                if (cursor < 0d)
                {
                    selected = value.value;
                    return true;
                }
            }

            selected = values.Last(value => value.weight > 0d).value;
            return true;
        }

        private readonly struct WeightedValue<T>
        {
            public WeightedValue(T value, double weight)
            {
                this.value = value;
                this.weight = weight;
            }

            public T value { get; }
            public double weight { get; }
        }
    }
}
