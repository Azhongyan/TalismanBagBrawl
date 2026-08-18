using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.BattleBridge.CampaignLoot
{
    public sealed class C1CampaignLootPool15BattleCandidateProfile
    {
        internal C1CampaignLootPool15BattleCandidateProfile(
            string baseItemId,
            string familyId,
            long amount,
            string nativeUnitId)
        {
            this.baseItemId = baseItemId ?? string.Empty;
            this.familyId = familyId ?? string.Empty;
            this.amount = amount;
            this.nativeUnitId = nativeUnitId ?? string.Empty;
            canonicalSignature = C1CampaignLootPool15BattleCanonical.Hash(
                C1CampaignLootPool15BattleCanonical.Build(builder =>
                {
                    builder.AddString("baseItemId", this.baseItemId);
                    builder.AddString("familyId", this.familyId);
                    builder.AddLong("amount", this.amount);
                    builder.AddString("nativeUnitId", this.nativeUnitId);
                }));
        }

        public string baseItemId { get; }
        public string familyId { get; }
        public long amount { get; }
        public string nativeUnitId { get; }
        public string canonicalSignature { get; }

        internal C1CampaignLootPool15BattleCandidateProfile Clone()
        {
            return new C1CampaignLootPool15BattleCandidateProfile(
                baseItemId,
                familyId,
                amount,
                nativeUnitId);
        }
    }

    public sealed class C1CampaignLootPool15BattleCandidateProfileSnapshot
    {
        private readonly ReadOnlyCollection<
            C1CampaignLootPool15BattleCandidateProfile> profiles;

        internal C1CampaignLootPool15BattleCandidateProfileSnapshot(
            IEnumerable<C1CampaignLootPool15BattleCandidateProfile> source)
        {
            schemaId = C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.SchemaId;
            profileId = C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.ProfileId;
            dataMaturity =
                C1CampaignLootPool15BattleEffectFamilyCandidateProfiles.DataMaturity;
            entersFormalFlow = false;
            profiles = Array.AsReadOnly((source
                ?? Enumerable.Empty<C1CampaignLootPool15BattleCandidateProfile>())
                .Where(row => row != null)
                .OrderBy(row => row.baseItemId, StringComparer.Ordinal)
                .Select(row => row.Clone())
                .ToArray());
            canonicalSignature = C1CampaignLootPool15BattleCanonical.Hash(
                C1CampaignLootPool15BattleCanonical.Build(builder =>
                {
                    builder.AddString("schemaId", schemaId);
                    builder.AddString("profileId", profileId);
                    builder.AddString("dataMaturity", dataMaturity);
                    builder.AddBool("entersFormalFlow", entersFormalFlow);
                    builder.AddInt("profileCount", profiles.Count);
                    foreach (C1CampaignLootPool15BattleCandidateProfile row in profiles)
                    {
                        builder.AddString("profileCanonicalSignature",
                            row.canonicalSignature);
                    }
                }));
        }

        public string schemaId { get; }
        public string profileId { get; }
        public string dataMaturity { get; }
        public bool entersFormalFlow { get; }
        public IReadOnlyList<C1CampaignLootPool15BattleCandidateProfile> Profiles =>
            profiles;
        public string canonicalSignature { get; }

        public C1CampaignLootPool15BattleCandidateProfile Find(string baseItemId)
        {
            C1CampaignLootPool15BattleCandidateProfile row = profiles.FirstOrDefault(
                candidate => string.Equals(
                    candidate.baseItemId,
                    baseItemId,
                    StringComparison.Ordinal));
            return row == null ? null : row.Clone();
        }

        internal C1CampaignLootPool15BattleCandidateProfileSnapshot Clone()
        {
            return new C1CampaignLootPool15BattleCandidateProfileSnapshot(profiles);
        }
    }

    public static class C1CampaignLootPool15BattleEffectFamilyCandidateProfiles
    {
        public const string SchemaId =
            "C1CampaignLootPool15BattleEffectFamilyCandidateProfile.v1";
        public const string ProfileId =
            "C1_CAMPAIGN_LV1_POOL15_BATTLE_EFFECT_CANDIDATE_R1";
        public const string DataMaturity = "CANDIDATE";
        public const string ExpectedPoolCanonicalSignature =
            "572c3421a4874ef203634e64896f59950c5746aa023026fc2503a643b99a47a0";
        public const string ExpectedSourceCatalogCanonicalSignature =
            "85e67946c6394a9977238a74c53969190b2472214b67bc15cec4b3f3668b0bdf";

        private static readonly C1CampaignLootPool15BattleCandidateProfileSnapshot
            Snapshot = new C1CampaignLootPool15BattleCandidateProfileSnapshot(
                new[]
                {
                    P("I002", "DIRECT_TEMPO", 1L, "point"),
                    P("I007", "DIRECT_TEMPO", 1L, "stack"),
                    P("I026", "DIRECT_TEMPO", 800L, "basisPoint"),
                    P("I004", "TARGET_SPREAD", 1L, "count"),
                    P("I011", "TARGET_SPREAD", 1L, "stack"),
                    P("I030", "TARGET_SPREAD", 1L, "count"),
                    P("I013", "GUARD_SUSTAIN", 4L, "flat"),
                    P("I015", "GUARD_SUSTAIN", 8L, "flat"),
                    P("I024", "GUARD_SUSTAIN", 12L, "flat"),
                    P("I003", "ENEMY_CONTROL", 650L, "basisPoint"),
                    P("I025", "ENEMY_CONTROL", 1L, "turn"),
                    P("I028", "ENEMY_CONTROL", 1L, "count"),
                    P("I010", "POSITIONAL_ADJACENCY", 1L, "point"),
                    P("I014", "POSITIONAL_ADJACENCY", 400L, "basisPoint"),
                    P("I022", "POSITIONAL_ADJACENCY", 1L, "turn")
                });

        public static C1CampaignLootPool15BattleCandidateProfileSnapshot Resolve()
        {
            return Snapshot.Clone();
        }

        private static C1CampaignLootPool15BattleCandidateProfile P(
            string baseItemId,
            string familyId,
            long amount,
            string nativeUnitId)
        {
            return new C1CampaignLootPool15BattleCandidateProfile(
                baseItemId,
                familyId,
                amount,
                nativeUnitId);
        }
    }
}
