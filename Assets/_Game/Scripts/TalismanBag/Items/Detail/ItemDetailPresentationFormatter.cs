using System;
using System.Globalization;
using TalismanBag.Items.Generation.Affixes;
using TalismanBag.Items.Generation.Stats;

namespace TalismanBag.Items.Detail
{
    /// <summary>
    /// The single display-value formatter used by Item detail projections.
    /// It only formats existing schema facts and never derives gameplay values.
    /// </summary>
    public static class ItemDetailPresentationFormatter
    {
        public const string ItemPowerUnavailable = "尚未建立正式评分";
        public const string EffectPayloadUnavailable = "效果数据尚未配置";
        public const string InactiveStyleStartToken = "[ItemDetailStyle:Inactive]";
        public const string ArrayModifierActiveStyleStartToken = "[ItemDetailStyle:ArrayModifierActive]";
        public const string ArrayModifierInactiveStyleStartToken = "[ItemDetailStyle:ArrayModifierInactive]";
        public const string StyleEndToken = "[/ItemDetailStyle]";

        private static readonly string[] SemanticStyleStartTokens =
        {
            InactiveStyleStartToken,
            ArrayModifierActiveStyleStartToken,
            ArrayModifierInactiveStyleStartToken
        };

        public static string MarkInactive(string value)
        {
            return InactiveStyleStartToken + (value ?? string.Empty) + StyleEndToken;
        }

        public static string MarkArrayModifier(string value, bool active)
        {
            return (active ? ArrayModifierActiveStyleStartToken : ArrayModifierInactiveStyleStartToken)
                + (value ?? string.Empty)
                + StyleEndToken;
        }

        public static bool ContainsInactiveStyle(string value)
        {
            return !string.IsNullOrEmpty(value)
                && (value.Contains(InactiveStyleStartToken, StringComparison.Ordinal)
                    || value.Contains(ArrayModifierInactiveStyleStartToken, StringComparison.Ordinal));
        }

        public static bool IsWholeLineInactiveStyle(string value)
        {
            string content = (value ?? string.Empty).Trim();
            return content.StartsWith(InactiveStyleStartToken, StringComparison.Ordinal)
                && content.EndsWith(StyleEndToken, StringComparison.Ordinal);
        }

        public static string StripSemanticStyleTokens(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            string result = value;
            foreach (string token in SemanticStyleStartTokens)
            {
                result = result.Replace(token, string.Empty);
            }

            return result.Replace(StyleEndToken, string.Empty);
        }

        public static string FormatStat(long rawUnits, ItemStatDefinitionSnapshot definition)
        {
            return Format(rawUnits, definition?.decimalPlaces ?? 0, definition?.unitKey, false);
        }

        public static string FormatStatRange(long minUnits, long maxUnits, ItemStatDefinitionSnapshot definition)
        {
            return FormatRange(minUnits, maxUnits, definition?.decimalPlaces ?? 0, definition?.unitKey, false);
        }

        public static string FormatAffix(long rawUnits, ItemAffixDefinitionSnapshot definition)
        {
            return Format(rawUnits, definition?.decimalPlaces ?? 0, definition?.valueUnitKey, true);
        }

        public static string FormatAffixRange(long minUnits, long maxUnits, ItemAffixDefinitionSnapshot definition)
        {
            return FormatRange(minUnits, maxUnits, definition?.decimalPlaces ?? 0, definition?.valueUnitKey, true);
        }

        public static string Format(long rawUnits, int decimalPlaces, string unitKey, bool signed)
        {
            string key = Normalize(unitKey).ToLowerInvariant();
            decimal value;
            string suffix;

            switch (key)
            {
                case "basispoint":
                case "basispoints":
                    value = rawUnits / 100m;
                    suffix = "%";
                    break;
                default:
                    value = rawUnits / Pow10(decimalPlaces);
                    suffix = UnitSuffix(key);
                    break;
            }

            string number = value.ToString("0.####", CultureInfo.InvariantCulture);
            string prefix = signed && value > 0m ? "+" : string.Empty;
            return prefix + number + suffix;
        }

        public static string UnitHint(string unitKey)
        {
            return Normalize(unitKey).ToLowerInvariant() switch
            {
                "basispoint" => "百分比",
                "basispoints" => "百分比",
                "percent" => "百分比",
                "percentage" => "百分比",
                "stack" => "层数",
                "point" => "点数",
                "turn" => "回合",
                "seconds" => "秒",
                "second" => "秒",
                "milliseconds" => "毫秒",
                _ => string.Empty
            };
        }

        private static string FormatRange(
            long minUnits,
            long maxUnits,
            int decimalPlaces,
            string unitKey,
            bool signed)
        {
            return Format(minUnits, decimalPlaces, unitKey, signed)
                + " ～ "
                + Format(maxUnits, decimalPlaces, unitKey, signed);
        }

        private static decimal Pow10(int decimalPlaces)
        {
            decimal scale = 1m;
            for (int index = 0; index < Math.Max(0, decimalPlaces); index++)
            {
                scale *= 10m;
            }

            return scale;
        }

        private static string UnitSuffix(string normalizedUnitKey)
        {
            return normalizedUnitKey switch
            {
                "percent" => "%",
                "percentage" => "%",
                "stack" => "层",
                "point" => "点",
                "turn" => "回合",
                "seconds" => "秒",
                "second" => "秒",
                "milliseconds" => "毫秒",
                "flat" => string.Empty,
                "" => string.Empty,
                _ => " " + normalizedUnitKey
            };
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
