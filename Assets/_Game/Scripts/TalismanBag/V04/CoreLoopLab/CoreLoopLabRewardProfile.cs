using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.Items.InnerCatalog;
using UnityEngine;

namespace TalismanBag.V04.CoreLoopLab
{
    public sealed class CoreLoopLabRewardCandidate
    {
        public CoreLoopLabRewardCandidate(
            string baseItemId,
            string authoritativeIdentityId,
            Color cardColor)
        {
            BaseItemId = baseItemId ?? string.Empty;
            AuthoritativeIdentityId = authoritativeIdentityId ?? string.Empty;
            CardColor = cardColor;
        }

        public string BaseItemId { get; }
        public string AuthoritativeIdentityId { get; }
        public Color CardColor { get; }
    }

    public sealed class CoreLoopLabRewardProfile
    {
        private readonly ReadOnlyCollection<CoreLoopLabRewardCandidate>
            rewardCandidates;
        private readonly ReadOnlyCollection<string> rewardCandidateBaseItemIds;
        private readonly ReadOnlyCollection<string> canonicalInitialRoster;
        private readonly ReadOnlyCollection<string>
            canonicalInitialAuthoritativeIdentities;

        private CoreLoopLabRewardProfile()
        {
            rewardCandidates = Array.AsReadOnly(new[]
            {
                new CoreLoopLabRewardCandidate(
                    "I007",
                    "wb_i007_orange_404310007",
                    new Color(0.86f, 0.31f, 0.18f, 1f)),
                new CoreLoopLabRewardCandidate(
                    "I009",
                    "wb_i009_orange_404310009",
                    new Color(0.68f, 0.55f, 0.24f, 1f)),
                new CoreLoopLabRewardCandidate(
                    "I012",
                    "wb_i012_orange_404310012",
                    new Color(0.20f, 0.56f, 0.82f, 1f))
            });
            canonicalInitialRoster = Array.AsReadOnly(new[]
            {
                "I008", "I010", "I011", "I031"
            });
            canonicalInitialAuthoritativeIdentities = Array.AsReadOnly(new[]
            {
                "SPECIAL_I031",
                "wb_i008_orange_404310008",
                "wb_i010_orange_404310010",
                "wb_i011_orange_404310011"
            });
            rewardCandidateBaseItemIds = Array.AsReadOnly(
                rewardCandidates.Select(value => value.BaseItemId).ToArray());

            string[] allIds = canonicalInitialRoster
                .Concat(rewardCandidateBaseItemIds)
                .ToArray();
            if (allIds.Length != 7
                || allIds.Distinct(StringComparer.Ordinal).Count() != 7
                || allIds.Any(id => ItemInnerDataCatalog.FindById(id) == null)
                || rewardCandidateBaseItemIds.Contains(
                    "I016",
                    StringComparer.Ordinal)
                || rewardCandidateBaseItemIds.Contains(
                    "I022",
                    StringComparer.Ordinal))
            {
                throw new InvalidOperationException(
                    "CORE_LOOP_LAB_CONTROLLED_ITEM_UNIVERSE_INVALID");
            }
        }

        public static CoreLoopLabRewardProfile Shared { get; } = new();

        public IReadOnlyList<CoreLoopLabRewardCandidate> RewardCandidates =>
            rewardCandidates;
        public IReadOnlyList<string> RewardCandidateBaseItemIds =>
            rewardCandidateBaseItemIds;
        public IReadOnlyList<string> CanonicalInitialRoster =>
            canonicalInitialRoster;
        public IReadOnlyList<string> CanonicalInitialAuthoritativeIdentities =>
            canonicalInitialAuthoritativeIdentities;

        public CoreLoopLabRewardCandidate FindCandidate(string baseItemId)
        {
            return rewardCandidates.FirstOrDefault(candidate => string.Equals(
                candidate.BaseItemId,
                baseItemId,
                StringComparison.Ordinal));
        }

        public ItemInnerDataDefinition ResolveAuthoritativeDefinition(
            string baseItemId)
        {
            string id = baseItemId ?? string.Empty;
            ItemInnerDataDefinition definition = ItemInnerDataCatalog.FindById(id);
            return definition != null
                   && string.Equals(definition.itemId, id,
                       StringComparison.Ordinal)
                ? definition
                : null;
        }
    }
}
