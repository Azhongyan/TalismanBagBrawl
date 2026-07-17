using System;
using TalismanBag.Items.Build;

namespace TalismanBag.ItemSandbox
{
    internal static class ItemSandboxBuildPresentationNames
    {
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

        public static string TrackDisplayName(ItemBuildTrackResult track)
        {
            if (track == null)
            {
                return "未命名构筑";
            }

            return track.trackKind == ItemBuildTrackKind.FaMen
                ? FaMenSetName(track.stableTag)
                : NormalizeQiLeiTrackName(track.stableTag);
        }

        public static string CategoryName(ItemBuildTrackResult track)
        {
            return track?.trackKind == ItemBuildTrackKind.FaMen ? "法门" : "器类";
        }

        public static string ProgressLabel(bool faMen, string stableTag)
        {
            if (faMen)
            {
                return "法门构筑";
            }

            string qiLeiName = NormalizeQiLeiTrackName(stableTag);
            return string.Equals(qiLeiName, "器类", StringComparison.Ordinal)
                ? "器类构筑"
                : qiLeiName + "类构筑";
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

        private static string NormalizeStableTag(string stableTag)
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
    }
}
