using System;
using System.Collections.Generic;
using TalismanBag.Items.Generation;

namespace TalismanBag.Items.Balance
{
    public enum ItemCandidateEffectCategory
    {
        NumericModifier, ConditionalModifier, TriggeredEffect, ResourceEffect, CooldownEffect,
        SpatialEffect, BuildSynergyEffect, CoreEffect, MechanicConversion, TradeoffEffect
    }

    public enum ItemCandidateEffectOperation
    {
        AddFlat, AddPercent, Multiply, ReduceFlat, ReducePercent, Refund,
        ExtraTarget, ExtraTrigger, Convert, Override
    }

    public enum ItemPresentationConfirmationState
    {
        Unconfirmed = 0,
        Candidate = 1,
        Confirmed = 2
    }

    [Serializable]
    public sealed class ItemCandidateEffectPayload
    {
        public string effectId = string.Empty;
        public string displayName = string.Empty;
        public string description = string.Empty;
        public ItemCandidateEffectCategory effectCategory;
        public string triggerEventId = string.Empty;
        public string conditionId = string.Empty;
        public string targetSelector = string.Empty;
        public string targetStatId = string.Empty;
        public ItemCandidateEffectOperation operation;
        public string valueUnitKey = string.Empty;
        public long valueUnits;
        public long secondaryValueUnits;
        public long durationUnits;
        public int stackLimit = 1;
        public long internalCooldownUnits;
        public string statusKey = string.Empty;
        public string statusFamilyKey = string.Empty;
        public string lifetimePolicy = "NONE";
        public string reapplyPolicy = "NONE";
        public string timeUnitKey = string.Empty;
        public long firstTickDelayUnits;
        public long tickIntervalUnits;
        public string tickSchedulePolicy = "NONE";
        public string tickDamageFormulaId = string.Empty;
        public long tickDamageValueUnits;
        public long tickDamageRatioBasisPoints;
        public bool canCrit;
        public bool countsAsHit;
        public bool autoConsumeAtMaxStack;
        public string consumeRule = "NONE";
        public string expireRule = "NONE";
        public string cleanseRule = "NONE";
        public string parameterProfileId = string.Empty;
        public string mutexGroupId = "NONE";
        public List<string> effectTags = new();
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public string designNote = string.Empty;
    }

    [Serializable]
    public sealed class ItemCandidateRarityValue
    {
        public ItemInstanceRarity rarity;
        public long minUnits;
        public long maxUnits;
    }

    [Serializable]
    public sealed class ItemCandidateAffixDefinition
    {
        public string affixId = string.Empty;
        public string displayName = string.Empty;
        public string description = string.Empty;
        public ItemCandidateEffectPayload effectPayload = new();
        public List<ItemCandidateRarityValue> rarityRanges = new();
        public string mutexGroupId = "NONE";
        public string repeatPolicy = "NO_DUPLICATE";
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public string designNote = string.Empty;
    }

    [Serializable]
    public sealed class ItemCandidateBuildStageDefinition
    {
        public string buildId = string.Empty;
        public string stableTag = string.Empty;
        public string displayName = string.Empty;
        public int stagePieceCount;
        public string description = string.Empty;
        public ItemCandidateEffectPayload effectPayload = new();
        public string conditionText = string.Empty;
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
        public string designNote = string.Empty;
    }

    [Serializable]
    public sealed class ItemCandidateDisplayProfile
    {
        public string triggerDescription = string.Empty;
        public string basicEffectDescription = string.Empty;
        public string lightingDescription = string.Empty;
        public string placementRecommendation = string.Empty;
        public string flavorText = string.Empty;
        public string faMenDisplayName = string.Empty;
        public string qiLeiDisplayName = string.Empty;
        public string shapeDescription = string.Empty;
        public string iconKey = string.Empty;
        public ItemPresentationConfirmationState presentationState =
            ItemPresentationConfirmationState.Unconfirmed;
        public string effectFamilyKey = string.Empty;
        public string presentationStyleKey = string.Empty;
        public string cueIdentity = string.Empty;
        public string dataMaturity = ItemBalanceWorkbenchCatalog.BalanceCandidate;
    }

    [Serializable]
    public sealed class ItemCandidateItemPowerCoefficient
    {
        public string coefficientId = string.Empty;
        public float value = 1f;
        public string description = string.Empty;
    }
}
