using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.EnemySystem.BoneAspect.CampaignBalance;
using TalismanBag.Items.Build;
using TalismanBag.Items.Canonical;
using TalismanBag.V04.ChapterFlow.Chapter1;

namespace TalismanBag.V04.RewardDrop.Runtime.CampaignLoot
{
    public static class C1CampaignDirectLootPoolAndPolicy
    {
        public const string ProductContext = "CAMPAIGN_NORMAL_LV1";
        public const string ChapterId = "bone_aspect_chapter_1";
        public const string RollSlotId = "c1.campaign.direct-loot.slot.ordinary.v1";
        public const int RecentExclusionLength = 2;
        public const int GrantQuantity = 1;
        public const int SourceRuleVersion = 1;
        public const string CanonicalPoolId = CanonicalItemCatalogContract.CatalogId;
        public const string CanonicalPoolVersion = CanonicalItemCatalogContract.CatalogVersion;
        public const string CanonicalRarityProfileId = "canonical-item-drop-playtest-v1";
        public const string CanonicalAttributeProfileId = "canonical-item-instance-roll-v1";
        public const string CanonicalRollPolicyVersion =
            "canonical-rarity-first-duplicate-decay-v1";
        public const string CanonicalAlgorithmId = "canonical-item-drop-policy";

        private static readonly ReadOnlyCollection<string> StableEligibleStages =
            Array.AsReadOnly(new[] { "1-1", "1-2", "1-3", "1-4", "1-5" });

        public static IReadOnlyList<string> EligibleStageIds => StableEligibleStages;

        public static CanonicalItemDropResult SelectCanonical(
            CanonicalItemDefinitionResolver resolver,
            string requestId,
            string itemInstanceId,
            string stageId,
            int acceptedDropOrdinal,
            long rootSeed,
            IReadOnlyDictionary<string, int> currentRunCopies,
            ItemBuildSynergyResolutionResult currentBuild)
        {
            int stageIndex = StableEligibleStages.IndexOf(stageId) + 1;
            if (resolver == null
                || string.IsNullOrWhiteSpace(requestId)
                || string.IsNullOrWhiteSpace(itemInstanceId)
                || stageIndex < 1
                || acceptedDropOrdinal < 1)
            {
                return null;
            }

            return CanonicalItemDropPolicy.Roll(
                resolver,
                new CanonicalItemDropRequest(
                    requestId,
                    itemInstanceId,
                    ProductContext,
                    stageId,
                    acceptedDropOrdinal,
                    stageIndex,
                    rootSeed,
                    currentRunCopies,
                    currentBuild,
                    ResolveNextBattleCapabilities(stageIndex)));
        }

        private static IReadOnlyList<string> ResolveNextBattleCapabilities(
            int clearedStageIndex)
        {
            if (clearedStageIndex < 1
                || clearedStageIndex >= StableEligibleStages.Count)
            {
                return Array.Empty<string>();
            }

            string nextStageId = StableEligibleStages[clearedStageIndex];
            V04Chapter1StageWavePlanDefinition nextEncounter =
                V04Chapter1StageWaveEncounterTruth.FindPlan(nextStageId);
            bool hasCleanseableThreat = nextEncounter?.waves
                .SelectMany(wave => wave.actors)
                .Select(actor => C1FormalEnemyDefinitionCatalog.FindByContentId(
                    actor.enemyContentId))
                .Any(DeclaresCleanseableThreat) == true;
            return hasCleanseableThreat
                ? new[]
                {
                    CanonicalItemBattleCapabilities.PlayerCleanseableStatus
                }
                : Array.Empty<string>();
        }

        private static bool DeclaresCleanseableThreat(
            C1FormalEnemyDefinition enemy)
        {
            return enemy?.Skills.Any(skill => skill?.Effects.Any(effect =>
                effect != null
                && string.Equals(
                    effect.EffectKind,
                    C1FormalEnemySkillEffectKinds.ApplyStatus,
                    StringComparison.Ordinal)
                && effect.Status?.Cleanseable == true) == true) == true;
        }
    }

    internal static class C1CampaignDirectLootCanonical
    {
        internal static string Build(params string[] fields)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string value in fields ?? Array.Empty<string>())
            {
                string normalized = value ?? string.Empty;
                builder.Append(normalized.Length.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(normalized);
                builder.Append('|');
            }
            return builder.ToString();
        }
    }
}
