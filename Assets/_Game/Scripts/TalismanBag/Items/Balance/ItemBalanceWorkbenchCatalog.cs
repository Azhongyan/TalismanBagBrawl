using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Generation;
using TalismanBag.Items.Generation.Potential;
using TalismanBag.Items.Generation.Rolling;
using TalismanBag.Items.Generation.Stats;
using UnityEngine;

namespace TalismanBag.Items.Balance
{
    [Serializable]
    public sealed class ItemBalanceStatDefinition
    {
        public string statId = string.Empty;
        public string displayName = string.Empty;
        public string unitKey = string.Empty;
        public ItemStatDirection direction;
        public int decimalPlaces;
        public long stepUnits = 1;
        public ItemStatRoundingMode roundingMode = ItemStatRoundingMode.Nearest;
    }

    [Serializable]
    public sealed class ItemBalanceBandSlice
    {
        public ItemInstanceRarity rarity;
        [Range(0f, 1f)] public float min01;
        [Range(0f, 1f)] public float max01 = 1f;
    }

    [Serializable]
    public sealed class ItemBalanceAffixDefinition
    {
        public string affixId = string.Empty;
        public string displayName = string.Empty;
        public string unitKey = string.Empty;
        public ItemStatDirection direction = ItemStatDirection.HigherIsBetter;
        public int decimalPlaces;
        public long stepUnits = 1;
        public ItemStatRoundingMode roundingMode = ItemStatRoundingMode.Nearest;
        public List<ItemBalanceAffixRarityRange> rarityRanges = new();
    }

    [Serializable]
    public sealed class ItemBalanceAffixRarityRange
    {
        public ItemInstanceRarity rarity;
        public long minUnits;
        public long maxUnits;
    }

    [Serializable]
    public sealed class ItemBalanceBuildPolicy
    {
        public ItemInstanceRarity rarity;
        public long noneWeight;
        public long faMenWeight;
        public long qiLeiWeight;
        public long dualWeight;

        public long TotalPositiveWeight => Math.Max(0, noneWeight) + Math.Max(0, faMenWeight)
            + Math.Max(0, qiLeiWeight) + Math.Max(0, dualWeight);
    }

    [Serializable]
    public sealed class ItemBalanceDropSegment
    {
        public int minStage = 1;
        public int maxStage = 10;
        public bool openEnded;
        public long whiteWeight = 100;
        public long greenWeight;
        public long blueWeight;
        public long purpleWeight;
        public long orangeWeight;
    }

    [Serializable]
    public sealed class ItemBalancePreviewCoefficient
    {
        public string dimensionId = string.Empty;
        public string statId = string.Empty;
        public float coefficient = 1f;
    }

    [CreateAssetMenu(menuName = "Talisman Bag/V0.4/Item Balance Workbench Catalog",
        fileName = "ItemBalanceWorkbenchCatalog")]
    public sealed class ItemBalanceWorkbenchCatalog : ScriptableObject
    {
        public const string CatalogAssetPath =
            "Assets/_Game/Configs/ItemBalanceWorkbench/ItemBalanceWorkbenchCatalog.asset";
        public const string BalanceCandidate = "BALANCE_CANDIDATE";
        public const string Editable = "EDITABLE";
        public const string NotLiveLocked = "NOT_LIVE_LOCKED";
        public const string NotBattleConnected = "NOT_BATTLE_CONNECTED";
        public const string DevPreviewOnly = "DEV_PREVIEW_ONLY";
        public const string CompleteCandidateRevision =
            "ITEM_FOUR_CORE_CANDIDATE_DATA_CORRECTION01_R1";

        public string balanceDataRevision = string.Empty;
        public string dataMaturity = BalanceCandidate;
        public string editState = Editable;
        public string liveState = NotLiveLocked;
        public string battleState = NotBattleConnected;
        public List<ItemBalanceStatDefinition> statDefinitions = new();
        public List<ItemBalanceBandSlice> higherBandTemplate = new();
        public List<ItemBalanceBandSlice> lowerBandTemplate = new();
        public List<ItemBalanceAffixDefinition> affixDefinitions = new();
        public List<ItemBalanceBuildPolicy> buildPolicies = new();
        public List<ItemBalanceDropSegment> dropSegments = new();
        public List<ItemBalancePreviewCoefficient> previewCoefficients = new();
        public List<ItemCandidateItemPowerCoefficient> candidateItemPowerCoefficients = new();
        public List<ItemCandidateAffixDefinition> candidateRandomAffixes = new();
        public List<ItemCandidateBuildStageDefinition> faMenBuildEffects = new();
        public List<ItemCandidateBuildStageDefinition> qiLeiBuildEffects = new();
        public List<ItemBalanceProfile> profiles = new();

        public ItemCandidateAffixDefinition FindCandidateAffix(string affixId)
        {
            return candidateRandomAffixes.FirstOrDefault(value => value != null
                && string.Equals(value.affixId, affixId, StringComparison.Ordinal));
        }

        public IEnumerable<ItemCandidateBuildStageDefinition> FindBuildStages(string stableTag, bool faMen)
        {
            return (faMen ? faMenBuildEffects : qiLeiBuildEffects).Where(value => value != null
                && string.Equals(value.stableTag, stableTag, StringComparison.Ordinal))
                .OrderBy(value => value.stagePieceCount);
        }

        public ItemBalanceProfile FindProfile(string baseItemId)
        {
            return profiles.FirstOrDefault(value => value != null
                && string.Equals(value.baseItemId, baseItemId, StringComparison.Ordinal));
        }

        public ItemBalanceStatDefinition FindStat(string statId)
        {
            return statDefinitions.FirstOrDefault(value => value != null
                && string.Equals(value.statId, statId, StringComparison.Ordinal));
        }

        public ItemBalanceAffixDefinition FindAffix(string affixId)
        {
            return affixDefinitions.FirstOrDefault(value => value != null
                && string.Equals(value.affixId, affixId, StringComparison.Ordinal));
        }

        public ItemBalanceBuildPolicy FindBuildPolicy(ItemInstanceRarity rarity)
        {
            return buildPolicies.FirstOrDefault(value => value != null && value.rarity == rarity);
        }

        public ItemBalanceBandSlice FindBand(ItemStatDirection direction, ItemInstanceRarity rarity)
        {
            List<ItemBalanceBandSlice> source = direction == ItemStatDirection.LowerIsBetter
                ? lowerBandTemplate
                : higherBandTemplate;
            return source.FirstOrDefault(value => value != null && value.rarity == rarity);
        }

        public static IEnumerable<ItemBuildQualificationRollEntry> ToBuildEntries(ItemBalanceBuildPolicy policy)
        {
            if (policy == null) yield break;
            if (policy.noneWeight > 0) yield return new ItemBuildQualificationRollEntry(ItemBuildQualification.None, policy.noneWeight);
            if (policy.faMenWeight > 0) yield return new ItemBuildQualificationRollEntry(ItemBuildQualification.FaMenOnly, policy.faMenWeight);
            if (policy.qiLeiWeight > 0) yield return new ItemBuildQualificationRollEntry(ItemBuildQualification.QiLeiOnly, policy.qiLeiWeight);
            if (policy.dualWeight > 0) yield return new ItemBuildQualificationRollEntry(ItemBuildQualification.Dual, policy.dualWeight);
        }
    }
}
