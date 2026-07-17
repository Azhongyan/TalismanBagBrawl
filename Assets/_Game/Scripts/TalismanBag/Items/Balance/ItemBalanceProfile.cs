using System;
using System.Collections.Generic;
using System.Linq;
using TalismanBag.Items.Generation;
using UnityEngine;

namespace TalismanBag.Items.Balance
{
    [Serializable]
    public sealed class ItemBalanceRange
    {
        public string statId = string.Empty;
        public long minUnits;
        public long maxUnits;

        public ItemBalanceRange Clone()
        {
            return new ItemBalanceRange { statId = statId, minUnits = minUnits, maxUnits = maxUnits };
        }
    }

    [Serializable]
    public sealed class ItemBalanceRarityVersion
    {
        public ItemInstanceRarity rarity;
        public string versionKey = string.Empty;
        public string cultivationPotentialProfileId = string.Empty;
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public List<ItemBalanceRange> statRanges = new();
        public List<string> eligibleCoreEffectIds = new();
        public List<string> visibleCoreEffectIds = new();
        public long candidateItemPower;
        public bool candidateItemPowerOverridden;
        public string candidateDisplaySummary = string.Empty;

        public ItemBalanceRange FindRange(string statId)
        {
            return statRanges.FirstOrDefault(value =>
                value != null && string.Equals(value.statId, statId, StringComparison.Ordinal));
        }
    }

    [Serializable]
    public sealed class ItemBalanceWeightedAffix
    {
        public string affixId = string.Empty;
        public long weight = 1;
        public string mutexGroupId = "NONE";
        public string repeatPolicy = "NO_DUPLICATE";
        public string designNote = string.Empty;
    }

    [Serializable]
    public sealed class ItemBalanceCoreCandidate
    {
        public string coreEffectId = string.Empty;
        public string displayName = string.Empty;
        [TextArea(2, 4)] public string description = string.Empty;
        public bool isUltimate;
        public string nodeKind = string.Empty;
        public int unlockLevel;
        public ItemInstanceRarity requiredRarity;
        public string effectType = string.Empty;
        public ItemCandidateEffectPayload effectPayload = new();
        public string stateDescription = string.Empty;
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public string designNote = string.Empty;
    }

    [CreateAssetMenu(menuName = "Talisman Bag/V0.4/Item Balance Profile", fileName = "ItemBalanceProfile")]
    public sealed class ItemBalanceProfile : ScriptableObject
    {
        public string balanceDataRevision = string.Empty;
        public string baseItemId = string.Empty;
        public string displayName = string.Empty;
        public string faMenTag = string.Empty;
        public string qiLeiTag = string.Empty;
        public int qiLeiPosition = 1;
        public string primaryStatId = string.Empty;
        public string secondaryStatId = string.Empty;
        public string fixedAffixId = string.Empty;
        public string randomPoolId = string.Empty;
        public List<ItemBalanceWeightedAffix> randomAffixes = new();
        public ItemCandidateAffixDefinition signatureAffix = new();
        public ItemCandidateDisplayProfile candidateDisplay = new();
        public List<ItemBalanceCoreCandidate> coreCandidates = new();
        public List<ItemBalanceRarityVersion> rarityVersions = new();

        public ItemBalanceRarityVersion FindVersion(ItemInstanceRarity rarity)
        {
            return rarityVersions.FirstOrDefault(value => value != null && value.rarity == rarity);
        }
    }
}
