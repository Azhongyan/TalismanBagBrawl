using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using TalismanBag.Items.Build;
using TalismanBag.Items.InnerCatalog;

namespace TalismanBag.Items.Detail
{
    public sealed class ItemBuildPlayerPresentationStage
    {
        public ItemBuildPlayerPresentationStage(
            int stagePieceCount,
            string effectDescription,
            bool isActive)
        {
            this.stagePieceCount = stagePieceCount;
            this.effectDescription = effectDescription ?? string.Empty;
            this.isActive = isActive;
        }

        public int stagePieceCount { get; }
        public string effectDescription { get; }
        public bool isActive { get; }
    }

    /// <summary>
    /// Shared read-only formatter for the legacy player-facing Build presentation.
    /// Callers provide authoritative progress and stage activity; this type never
    /// evaluates Build qualification, thresholds, placement, lighting, or counts.
    /// </summary>
    public static class ItemBuildPlayerPresentationFormatter
    {
        public const string FaMenSectionTitle = "法门构筑";
        public const string QiLeiSectionTitle = "器类构筑";
        public const string BuildTitleHex = "F2EDE2";
        public const string BuildActiveHex = "87B66A";
        public const string BuildInactiveHex = "8A8A8A";

        public static string FormatTrack(
            bool faMen,
            string stableTag,
            int currentCount,
            int maxPieceCount,
            IEnumerable<ItemBuildPlayerPresentationStage> stages,
            IEnumerable<string> activeMemberBaseItemIds = null,
            string indent = "",
            string rarityColorHex = null,
            string faMenDisplayNameOverride = null)
        {
            string safeIndent = indent ?? string.Empty;
            ItemBuildPlayerPresentationStage[] orderedStages =
                (stages ?? Array.Empty<ItemBuildPlayerPresentationStage>())
                .Where(value => value != null)
                .OrderBy(value => value.stagePieceCount)
                .ToArray();
            string progressLine = safeIndent + BuildColoredText(
                ProgressLabel(faMen, stableTag)
                    + "：" + FormatProgress(currentCount, maxPieceCount),
                BuildActiveHex,
                true);
            string stageRows = string.Join(
                "\n",
                orderedStages.Select(value => FormatStageRow(
                    faMen,
                    stableTag,
                    value,
                    safeIndent,
                    rarityColorHex)));

            if (!faMen)
            {
                return progressLine
                    + (stageRows.Length == 0 ? string.Empty : "\n" + stageRows);
            }

            string displayName = string.IsNullOrWhiteSpace(faMenDisplayNameOverride)
                ? FaMenSetName(stableTag)
                : faMenDisplayNameOverride.Trim();
            string memberRows = FormatFaMenMemberRows(
                stableTag,
                activeMemberBaseItemIds,
                safeIndent);
            return safeIndent + BuildColoredText(
                    displayName,
                    BuildTitleHex,
                    true)
                + "\n" + progressLine
                + (memberRows.Length == 0 ? string.Empty : "\n" + memberRows)
                + (stageRows.Length == 0 ? string.Empty : "\n" + stageRows);
        }

        public static string FaMenSetName(string stableTag)
        {
            return NormalizeStableTag(stableTag) switch
            {
                "zhenlei" => "九霄雷君的敕令",
                "lihuo" => "南离天君的焚章",
                "zhongyue" => "中岳府君的镇章",
                "xuanshui" => "玄水真君的涤秽经",
                "taibai" => "太白星君的斩煞",
                _ => "未命名法门典藏"
            };
        }

        public static string QiLeiStageName(string stableTag, int stagePieceCount)
        {
            bool fourPiece = stagePieceCount >= 4;
            return NormalizeStableTag(stableTag) switch
            {
                "fu" => fourPiece ? "符阵成局" : "符箓相合",
                "yin" => fourPiece ? "印法成局" : "印契相合",
                "ling" => fourPiece ? "令文成局" : "令势相合",
                "jing" => fourPiece ? "镜照成局" : "镜光相合",
                "fa" => fourPiece ? "器法成局" : "法器相合",
                _ => fourPiece ? "器类成局" : "器类相合"
            };
        }

        public static string TrackDisplayName(
            ItemBuildTrackKind trackKind,
            string stableTag)
        {
            return trackKind == ItemBuildTrackKind.FaMen
                ? FaMenSetName(stableTag)
                : NormalizeQiLeiTrackName(stableTag);
        }

        public static string ProgressLabel(bool faMen, string stableTag)
        {
            if (faMen)
            {
                return FaMenSectionTitle;
            }

            string qiLeiName = NormalizeQiLeiTrackName(stableTag);
            return string.Equals(qiLeiName, "器类", StringComparison.Ordinal)
                ? QiLeiSectionTitle
                : qiLeiName + "类构筑";
        }

        public static string ResolveRarityColorHex(
            string rarityKey,
            string rarityColorKey)
        {
            string key = string.IsNullOrWhiteSpace(rarityColorKey)
                ? rarityKey
                : rarityColorKey;
            return (key ?? string.Empty).Trim().ToLowerInvariant() switch
            {
                "white" => "E3D8C3",
                "green" => "87B66A",
                "blue" => "68A9E6",
                "purple" => "B27DDF",
                "orange" => "E4A14B",
                _ => "D8CCB7"
            };
        }

        public static string NormalizeStableTag(string stableTag)
        {
            string value = (stableTag ?? string.Empty).Trim().ToLowerInvariant();
            if (value.StartsWith("famen:", StringComparison.Ordinal))
            {
                value = value.Substring("famen:".Length);
            }
            else if (value.StartsWith("qilei:", StringComparison.Ordinal))
            {
                value = value.Substring("qilei:".Length);
            }

            int colon = value.IndexOf(':');
            return colon >= 0 ? value.Substring(0, colon) : value;
        }

        private static string FormatFaMenMemberRows(
            string stableTag,
            IEnumerable<string> activeMemberBaseItemIds,
            string indent)
        {
            string normalizedTag = NormalizeStableTag(stableTag);
            if (string.IsNullOrWhiteSpace(normalizedTag))
            {
                return string.Empty;
            }

            HashSet<string> activeIds = new(
                activeMemberBaseItemIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            ItemInnerDataDefinition[] members = ItemInnerDataCatalog.AllItems
                .Where(item => item != null
                    && !item.isLightingSource
                    && string.Equals(
                        NormalizeStableTag(item.FaMenKey),
                        normalizedTag,
                        StringComparison.Ordinal))
                .OrderBy(item => item.itemId, StringComparer.Ordinal)
                .ToArray();
            return string.Join("\n", members.Select(item =>
            {
                bool active = activeIds.Contains(item.itemId);
                return indent + BuildColoredText(
                    "-" + item.displayName,
                    active ? BuildActiveHex : BuildInactiveHex,
                    active);
            }));
        }

        private static string FormatStageRow(
            bool faMen,
            string stableTag,
            ItemBuildPlayerPresentationStage stage,
            string indent,
            string rarityColorHex)
        {
            string detail = stage.effectDescription ?? string.Empty;
            if (!faMen)
            {
                detail = QiLeiStageName(stableTag, stage.stagePieceCount)
                    + "：" + NonEmpty(detail, "未配置");
            }
            else
            {
                detail = NonEmpty(detail, "未配置");
            }

            if (stage.isActive)
            {
                detail = HighlightBuildCoreValue(
                    detail,
                    string.IsNullOrWhiteSpace(rarityColorHex)
                        ? "D8CCB7"
                        : rarityColorHex);
            }

            string color = stage.isActive ? BuildActiveHex : BuildInactiveHex;
            return indent + BuildColoredText(
                    stage.stagePieceCount.ToString(CultureInfo.InvariantCulture)
                        + "件效果：",
                    color,
                    true)
                + "\n" + indent + BuildColoredText(detail, color, false);
        }

        private static string HighlightBuildCoreValue(
            string value,
            string colorHex)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value ?? string.Empty;
            }

            StringBuilder builder = new(value.Length + 32);
            bool highlighted = false;
            for (int index = 0; index < value.Length;)
            {
                if (!IsBuildValueStart(value, index))
                {
                    builder.Append(value[index]);
                    index++;
                    continue;
                }

                int end = index + 1;
                while (end < value.Length && !IsBuildValueTerminator(value[end]))
                {
                    end++;
                }

                builder.Append(BuildColoredText(
                    value.Substring(index, end - index),
                    colorHex,
                    true));
                highlighted = true;
                index = end;
            }

            return highlighted ? builder.ToString() : value;
        }

        private static bool IsBuildValueStart(string value, int index)
        {
            char current = value[index];
            if (current == '+' || current == '-' || current == '×')
            {
                return index + 1 < value.Length && char.IsDigit(value[index + 1]);
            }

            return char.IsDigit(current)
                && (index == 0 || char.IsWhiteSpace(value[index - 1]));
        }

        private static bool IsBuildValueTerminator(char value)
        {
            return char.IsWhiteSpace(value)
                || value == '；'
                || value == ';'
                || value == '，'
                || value == ','
                || value == '。'
                || value == ')'
                || value == '）';
        }

        private static string BuildColoredText(
            string value,
            string colorHex,
            bool bold)
        {
            string text = value ?? string.Empty;
            if (bold)
            {
                text = "<b>" + text + "</b>";
            }

            return "<color=#" + colorHex + ">" + text + "</color>";
        }

        private static string FormatProgress(int currentCount, int maxPieceCount)
        {
            return Math.Max(0, currentCount).ToString(CultureInfo.InvariantCulture)
                + "/"
                + Math.Max(0, maxPieceCount).ToString(CultureInfo.InvariantCulture);
        }

        private static string NormalizeQiLeiTrackName(string stableTag)
        {
            return NormalizeStableTag(stableTag) switch
            {
                "fu" => "符",
                "yin" => "印",
                "ling" => "令",
                "jing" => "镜",
                "fa" => "法",
                _ => "器类"
            };
        }

        private static string NonEmpty(string value, string fallback)
        {
            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
