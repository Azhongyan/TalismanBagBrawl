using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TalismanBag.Items.Generation;

namespace TalismanBag.Items.Detail
{
    public enum ItemDetailArrayModifierTargetKind
    {
        Stat = 0,
        Signature = 1,
        RandomAffix = 2,
        CoreEffect = 3,
        FaMenBuildStage = 4,
        QiLeiBuildStage = 5
    }

    public enum ItemDetailArrayModifierOperation
    {
        AddFlat = 0,
        AddPercent = 1,
        ReduceFlat = 2,
        ReducePercent = 3,
        ExtraTrigger = 4,
        ExtraTarget = 5,
        Convert = 6
    }

    public sealed class ItemDetailArrayModifierDefinition
    {
        public string modifierId;
        public string baseItemId;
        public string rarityVersionKey;
        public string sourceKind = ItemDetailArrayModifierResolver.SourceKind;
        public string sourceCellScope = "AnyArrayBonusCell";
        public string sourceCellId = "AP_ANY";
        public ItemDetailArrayModifierTargetKind targetKind;
        public string targetId;
        public string targetParameterKey;
        public ItemDetailArrayModifierOperation operation;
        public long rawUnits;
        public string unitKey;
        public int decimalPlaces;
        public string roundingMode = "HalfAwayFromZero";
        public string displayName;
        public string description;
        public string stackingPolicy = "Additive";
        public string dataMaturity = ItemDetailArrayModifierResolver.DataMaturity;
    }

    public sealed class ItemDetailResolvedArrayModifier
    {
        public ItemDetailResolvedArrayModifier(
            ItemDetailArrayModifierDefinition definition,
            bool isCurrentlyApplied,
            string formattedDelta,
            string iconKey,
            string colorToken)
        {
            this.definition = definition;
            this.isCurrentlyApplied = isCurrentlyApplied;
            isVisible = isCurrentlyApplied;
            formattedDelta = formattedDelta ?? string.Empty;
            this.iconKey = iconKey ?? string.Empty;
            this.colorToken = colorToken ?? string.Empty;
        }

        public ItemDetailResolvedArrayModifier(
            ItemDetailArrayModifierDefinition definition,
            bool isCurrentlyApplied,
            bool isVisible,
            string formattedDelta,
            string iconKey,
            string colorToken)
        {
            this.definition = definition;
            this.isCurrentlyApplied = isCurrentlyApplied;
            this.isVisible = isVisible;
            this.formattedDelta = formattedDelta ?? string.Empty;
            this.iconKey = iconKey ?? string.Empty;
            this.colorToken = colorToken ?? string.Empty;
        }

        public ItemDetailArrayModifierDefinition definition { get; }
        public bool isCurrentlyApplied { get; }
        public bool isVisible { get; }
        public string formattedDelta { get; }
        public string iconKey { get; }
        public string colorToken { get; }
        public string modifierId => definition?.modifierId ?? string.Empty;
        public ItemDetailArrayModifierTargetKind targetKind => definition?.targetKind ?? ItemDetailArrayModifierTargetKind.Stat;
        public string targetId => definition?.targetId ?? string.Empty;
    }

    public sealed class ItemDetailArrayModifierResolutionContext
    {
        public string baseItemId;
        public ItemInstanceRarity rarity;
        public bool isOnArrayBonusCell;
        public bool isArrayBonusActive;
        public IReadOnlyCollection<string> activeCoreEffectIds = Array.Empty<string>();
        public int faMenActiveStagePieceCount;
        public int qiLeiActiveStagePieceCount;
    }

    public static class ItemDetailArrayModifierResolver
    {
        public const string SourceKind = "ArrayBonus";
        public const string DataMaturity = "BALANCE_CANDIDATE";
        public const string Editable = "EDITABLE";
        public const string NotLiveLocked = "NOT_LIVE_LOCKED";
        public const string NotBattleConnected = "NOT_BATTLE_CONNECTED";
        public const string IconKey = "Icon_ArrayVeinModifier";
        public const string InlineIconToken = "[Icon_ArrayVeinModifier]";
        public const string ColorTokenName = "arrayModifierColor";
        private static readonly string[] ForbiddenTextIconFallbacks =
        {
            "\u25C6",
            char.ConvertFromUtf32(0x1F537),
            "\u2728"
        };

        private static readonly ItemInstanceRarity[] Rarities =
        {
            ItemInstanceRarity.White,
            ItemInstanceRarity.Green,
            ItemInstanceRarity.Blue,
            ItemInstanceRarity.Purple,
            ItemInstanceRarity.Orange
        };

        public static IReadOnlyList<ItemDetailArrayModifierDefinition> BuildDefaultCandidateDefinitions()
        {
            List<ItemDetailArrayModifierDefinition> definitions = new();
            for (int itemIndex = 1; itemIndex <= 30; itemIndex++)
            {
                string baseItemId = "I" + itemIndex.ToString("000", CultureInfo.InvariantCulture);
                for (int rarityIndex = 0; rarityIndex < Rarities.Length; rarityIndex++)
                {
                    ItemInstanceRarity rarity = Rarities[rarityIndex];
                    ItemDetailArrayModifierTargetKind targetKind =
                        (ItemDetailArrayModifierTargetKind)((itemIndex + rarityIndex - 1) % 6);
                    definitions.Add(BuildDefinition(baseItemId, rarity, itemIndex, rarityIndex, targetKind));
                }
            }

            return definitions;
        }

        public static ItemDetailArrayModifierDefinition CloneDefinition(ItemDetailArrayModifierDefinition source)
        {
            if (source == null)
            {
                return null;
            }

            return new ItemDetailArrayModifierDefinition
            {
                modifierId = source.modifierId,
                baseItemId = source.baseItemId,
                rarityVersionKey = source.rarityVersionKey,
                sourceKind = source.sourceKind,
                sourceCellScope = source.sourceCellScope,
                sourceCellId = source.sourceCellId,
                targetKind = source.targetKind,
                targetId = source.targetId,
                targetParameterKey = source.targetParameterKey,
                operation = source.operation,
                rawUnits = source.rawUnits,
                unitKey = source.unitKey,
                decimalPlaces = source.decimalPlaces,
                roundingMode = source.roundingMode,
                displayName = source.displayName,
                description = source.description,
                stackingPolicy = source.stackingPolicy,
                dataMaturity = source.dataMaturity
            };
        }

        public static bool IsCandidateDefinitionValid(ItemDetailArrayModifierDefinition definition)
        {
            return definition != null
                && !string.IsNullOrWhiteSpace(definition.modifierId)
                && !string.IsNullOrWhiteSpace(definition.baseItemId)
                && !string.IsNullOrWhiteSpace(definition.rarityVersionKey)
                && string.Equals(definition.sourceKind, SourceKind, StringComparison.Ordinal)
                && Enum.IsDefined(typeof(ItemDetailArrayModifierTargetKind), definition.targetKind)
                && Enum.IsDefined(typeof(ItemDetailArrayModifierOperation), definition.operation)
                && !string.IsNullOrWhiteSpace(definition.targetId)
                && !string.IsNullOrWhiteSpace(definition.targetParameterKey)
                && !string.IsNullOrWhiteSpace(definition.unitKey)
                && string.Equals(definition.dataMaturity, DataMaturity, StringComparison.Ordinal)
                && !string.Equals(definition.dataMaturity, "PLAYTEST_ACCEPTED", StringComparison.Ordinal)
                && !string.Equals(definition.dataMaturity, "LIVE_LOCKED", StringComparison.Ordinal);
        }

        public static IReadOnlyList<ItemDetailResolvedArrayModifier> Resolve(
            ItemDetailArrayModifierResolutionContext context,
            IEnumerable<ItemDetailArrayModifierDefinition> definitions = null)
        {
            if (context == null || string.IsNullOrWhiteSpace(context.baseItemId))
            {
                return Array.Empty<ItemDetailResolvedArrayModifier>();
            }

            string rarityVersionKey = BuildRarityVersionKey(context.baseItemId, context.rarity);
            IReadOnlyCollection<string> activeCoreIds = context.activeCoreEffectIds ?? Array.Empty<string>();

            return (definitions ?? BuildDefaultCandidateDefinitions())
                .Where(definition => IsDefinitionForContext(definition, context.baseItemId, rarityVersionKey))
                .Select(definition => new ItemDetailResolvedArrayModifier(
                    definition,
                    IsTargetCurrentlyEffective(definition, context, activeCoreIds),
                    true,
                    FormatDelta(definition),
                    IconKey,
                    ColorTokenName))
                .Where(modifier => modifier.isVisible)
                .ToArray();
        }

        public static string AppendInlineModifier(
            string source,
            ItemDetailResolvedArrayModifier modifier,
            bool useArrayModifierColor = true)
        {
            if (modifier == null || !modifier.isVisible)
            {
                return source ?? string.Empty;
            }

            string text = source ?? string.Empty;
            string inline = InlineIconToken + " " + modifier.formattedDelta;
            string rendered = ItemDetailPresentationFormatter.MarkArrayModifier(
                inline,
                useArrayModifierColor && modifier.isCurrentlyApplied);
            return text.Length == 0
                ? rendered
                : text + " " + rendered;
        }

        public static string FormatDelta(ItemDetailArrayModifierDefinition definition)
        {
            if (definition == null)
            {
                return string.Empty;
            }

            string value = FormatUnits(Math.Abs(definition.rawUnits), definition.unitKey);
            return definition.operation switch
            {
                ItemDetailArrayModifierOperation.ReduceFlat => "-" + value,
                ItemDetailArrayModifierOperation.ReducePercent => "-" + value,
                ItemDetailArrayModifierOperation.Convert => value,
                _ => "+" + value
            };
        }

        public static string BuildRarityVersionKey(string baseItemId, ItemInstanceRarity rarity)
        {
            return (baseItemId ?? string.Empty).Trim() + "@" + rarity.ToStableKey();
        }

        public static bool IsModifierIconTokenLegal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return value.Contains(InlineIconToken, StringComparison.Ordinal)
                && ForbiddenTextIconFallbacks.All(token => !value.Contains(token, StringComparison.Ordinal));
        }

        private static ItemDetailArrayModifierDefinition BuildDefinition(
            string baseItemId,
            ItemInstanceRarity rarity,
            int itemIndex,
            int rarityIndex,
            ItemDetailArrayModifierTargetKind targetKind)
        {
            long rawUnits = targetKind switch
            {
                ItemDetailArrayModifierTargetKind.Stat => 1 + rarityIndex,
                ItemDetailArrayModifierTargetKind.Signature => 200 + rarityIndex * 50L,
                ItemDetailArrayModifierTargetKind.RandomAffix => 100 + rarityIndex * 25L,
                ItemDetailArrayModifierTargetKind.CoreEffect => 150 + rarityIndex * 50L,
                ItemDetailArrayModifierTargetKind.FaMenBuildStage => rarityIndex < 2 ? 200 : 300,
                ItemDetailArrayModifierTargetKind.QiLeiBuildStage => rarityIndex < 2 ? 1 : 2,
                _ => 1
            };
            string unitKey = targetKind switch
            {
                ItemDetailArrayModifierTargetKind.Stat => "point",
                ItemDetailArrayModifierTargetKind.QiLeiBuildStage => "count",
                _ => "basisPoint"
            };
            ItemDetailArrayModifierOperation operation = targetKind switch
            {
                ItemDetailArrayModifierTargetKind.QiLeiBuildStage => ItemDetailArrayModifierOperation.ExtraTarget,
                ItemDetailArrayModifierTargetKind.FaMenBuildStage => ItemDetailArrayModifierOperation.AddPercent,
                ItemDetailArrayModifierTargetKind.CoreEffect => ItemDetailArrayModifierOperation.AddPercent,
                _ => ItemDetailArrayModifierOperation.AddFlat
            };

            return new ItemDetailArrayModifierDefinition
            {
                modifierId = "array_candidate_" + baseItemId.ToLowerInvariant()
                    + "_" + rarity.ToStableKey() + "_" + targetKind.ToString().ToLowerInvariant(),
                baseItemId = baseItemId,
                rarityVersionKey = BuildRarityVersionKey(baseItemId, rarity),
                targetKind = targetKind,
                targetId = ResolveTargetId(baseItemId, targetKind, rarityIndex),
                targetParameterKey = ResolveTargetParameterKey(targetKind),
                operation = operation,
                rawUnits = rawUnits,
                unitKey = unitKey,
                decimalPlaces = unitKey == "basisPoint" ? 2 : 0,
                displayName = "阵脉候选修正",
                description = "Sandbox 阵脉候选修正；可编辑、未接正式战斗。",
                stackingPolicy = "Additive",
                dataMaturity = DataMaturity
            };
        }

        private static string ResolveTargetId(string baseItemId, ItemDetailArrayModifierTargetKind targetKind, int rarityIndex)
        {
            return targetKind switch
            {
                ItemDetailArrayModifierTargetKind.Stat => "primaryStat",
                ItemDetailArrayModifierTargetKind.Signature => "signature_" + baseItemId.ToLowerInvariant(),
                ItemDetailArrayModifierTargetKind.RandomAffix => "randomAffix:0",
                ItemDetailArrayModifierTargetKind.CoreEffect => "candidate_core_" + baseItemId.ToLowerInvariant()
                    + "_" + Math.Max(1, Math.Min(4, rarityIndex + 1)).ToString("00", CultureInfo.InvariantCulture),
                ItemDetailArrayModifierTargetKind.FaMenBuildStage => "famen:stage:"
                    + (rarityIndex >= 4 ? "6" : rarityIndex >= 2 ? "4" : "2"),
                ItemDetailArrayModifierTargetKind.QiLeiBuildStage => "qilei:stage:"
                    + (rarityIndex >= 2 ? "4" : "2"),
                _ => string.Empty
            };
        }

        private static string ResolveTargetParameterKey(ItemDetailArrayModifierTargetKind targetKind)
        {
            return targetKind switch
            {
                ItemDetailArrayModifierTargetKind.Stat => "rawUnits",
                ItemDetailArrayModifierTargetKind.Signature => "signaturePayload",
                ItemDetailArrayModifierTargetKind.RandomAffix => "affixPayload",
                ItemDetailArrayModifierTargetKind.CoreEffect => "corePayload",
                ItemDetailArrayModifierTargetKind.FaMenBuildStage => "buildStagePayload",
                ItemDetailArrayModifierTargetKind.QiLeiBuildStage => "buildStagePayload",
                _ => "payload"
            };
        }

        private static bool IsDefinitionForContext(
            ItemDetailArrayModifierDefinition definition,
            string baseItemId,
            string rarityVersionKey)
        {
            return definition != null
                && string.Equals(definition.dataMaturity, DataMaturity, StringComparison.Ordinal)
                && string.Equals(definition.baseItemId, baseItemId, StringComparison.Ordinal)
                && string.Equals(definition.rarityVersionKey, rarityVersionKey, StringComparison.Ordinal);
        }

        private static bool IsTargetCurrentlyEffective(
            ItemDetailArrayModifierDefinition definition,
            ItemDetailArrayModifierResolutionContext context,
            IReadOnlyCollection<string> activeCoreIds)
        {
            if (!context.isArrayBonusActive)
            {
                return false;
            }

            if (definition == null)
            {
                return false;
            }

            if (definition.targetKind == ItemDetailArrayModifierTargetKind.CoreEffect)
            {
                return activeCoreIds.Contains(definition.targetId, StringComparer.Ordinal);
            }

            if (definition.targetKind == ItemDetailArrayModifierTargetKind.FaMenBuildStage)
            {
                return context.faMenActiveStagePieceCount >= ParseStage(definition.targetId);
            }

            if (definition.targetKind == ItemDetailArrayModifierTargetKind.QiLeiBuildStage)
            {
                return context.qiLeiActiveStagePieceCount >= ParseStage(definition.targetId);
            }

            return true;
        }

        private static int ParseStage(string targetId)
        {
            if (string.IsNullOrWhiteSpace(targetId))
            {
                return 0;
            }

            int index = targetId.LastIndexOf(':');
            return index >= 0 && int.TryParse(targetId.Substring(index + 1), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : 0;
        }

        private static string FormatUnits(long units, string unitKey)
        {
            return unitKey switch
            {
                "basisPoint" => (units / 100d).ToString("0.##", CultureInfo.InvariantCulture) + "%",
                "point" => units.ToString(CultureInfo.InvariantCulture) + "点",
                "flat" => units.ToString(CultureInfo.InvariantCulture),
                "count" => units.ToString(CultureInfo.InvariantCulture) + "次",
                "stack" => units.ToString(CultureInfo.InvariantCulture) + "层",
                "turn" => units.ToString(CultureInfo.InvariantCulture) + "回合",
                _ => units.ToString(CultureInfo.InvariantCulture)
            };
        }

    }
}
