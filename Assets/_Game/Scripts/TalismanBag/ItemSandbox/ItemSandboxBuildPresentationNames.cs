using System;
using TalismanBag.Items.Build;

namespace TalismanBag.ItemSandbox
{
    internal static class ItemSandboxBuildPresentationNames
    {
        public static string FaMenSetName(string stableTag)
        {
            return TalismanBag.Items.Detail.ItemBuildPlayerPresentationFormatter
                .FaMenSetName(stableTag);
        }

        public static string QiLeiStageName(string stableTag, int stagePieceCount)
        {
            return TalismanBag.Items.Detail.ItemBuildPlayerPresentationFormatter
                .QiLeiStageName(stableTag, stagePieceCount);
        }

        public static string TrackDisplayName(ItemBuildTrackResult track)
        {
            if (track == null)
            {
                return "未命名构筑";
            }

            return TalismanBag.Items.Detail.ItemBuildPlayerPresentationFormatter
                .TrackDisplayName(track.trackKind, track.stableTag);
        }

        public static string CategoryName(ItemBuildTrackResult track)
        {
            return track?.trackKind == ItemBuildTrackKind.FaMen ? "法门" : "器类";
        }

        public static string ProgressLabel(bool faMen, string stableTag)
        {
            return TalismanBag.Items.Detail.ItemBuildPlayerPresentationFormatter
                .ProgressLabel(faMen, stableTag);
        }
    }
}
