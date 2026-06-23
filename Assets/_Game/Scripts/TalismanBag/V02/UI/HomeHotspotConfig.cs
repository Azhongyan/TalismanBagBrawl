using System;
using System.Collections.Generic;

namespace TalismanBag.V02.UI
{
    [Serializable]
    public sealed class HomeHotspotConfig
    {
        public HomeHotspotId hotspotId;
        public string displayName;
        public HomeHotspotState state;
        public HomeHotspotTargetType targetType;
        public string visualKey;
        public string iconKey;
        public string lockedReason;
        public string comingSoonText;

        public HomeHotspotConfig(
            HomeHotspotId hotspotId,
            string displayName,
            HomeHotspotState state,
            HomeHotspotTargetType targetType,
            string visualKey = "",
            string iconKey = "",
            string lockedReason = "",
            string comingSoonText = "")
        {
            this.hotspotId = hotspotId;
            this.displayName = displayName ?? string.Empty;
            this.state = state;
            this.targetType = targetType;
            this.visualKey = visualKey ?? string.Empty;
            this.iconKey = iconKey ?? string.Empty;
            this.lockedReason = lockedReason ?? string.Empty;
            this.comingSoonText = comingSoonText ?? string.Empty;
        }

        public HomeHotspotConfig Clone()
        {
            return new HomeHotspotConfig(
                hotspotId,
                displayName,
                state,
                targetType,
                visualKey,
                iconKey,
                lockedReason,
                comingSoonText);
        }

        public static List<HomeHotspotConfig> CreateDefaultSet()
        {
            return new List<HomeHotspotConfig>
            {
                new(HomeHotspotId.Counter, "柜台", HomeHotspotState.Available, HomeHotspotTargetType.CurrentObjective),
                new(HomeHotspotId.Trial, "试炼", HomeHotspotState.Available, HomeHotspotTargetType.MainTrial),
                new(HomeHotspotId.Refine, "炼符", HomeHotspotState.Available, HomeHotspotTargetType.TalismanRefine),
                new(HomeHotspotId.Codex, "道藏", HomeHotspotState.Locked, HomeHotspotTargetType.Collection,
                    lockedReason: "道藏仍在整理，当前可先查看试炼与炼符。"),
                new(HomeHotspotId.BackRoom, "后屋", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    comingSoonText: "后屋尚未开放，后续用于生产养成。"),
                new(HomeHotspotId.DreamSign, "梦签", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    comingSoonText: "梦签尚未开放，后续承接每日签。"),
                new(HomeHotspotId.Explore, "罗盘台", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    comingSoonText: "罗盘台尚未开放，后续承接探索。"),
                new(HomeHotspotId.TianjiFurnace, "旧炉子", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    comingSoonText: "旧炉子暂未启用，天机炉玩法将在后续开放。"),
                new(HomeHotspotId.MasterRelic, "师父旧物", HomeHotspotState.Locked, HomeHotspotTargetType.StoryAnchor,
                    lockedReason: "旧物上的禁制尚未松动。"),
                new(HomeHotspotId.PvpPlaceholder, "切磋占位", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    comingSoonText: "切磋功能尚未开放。"),
                new(HomeHotspotId.Store, "商店", HomeHotspotState.ComingSoon, HomeHotspotTargetType.SystemShortcut,
                    comingSoonText: "商店快捷入口尚未开放。"),
                new(HomeHotspotId.Mail, "邮件", HomeHotspotState.ComingSoon, HomeHotspotTargetType.SystemShortcut,
                    comingSoonText: "邮件快捷入口尚未开放。"),
                new(HomeHotspotId.Notice, "公告", HomeHotspotState.ComingSoon, HomeHotspotTargetType.SystemShortcut,
                    comingSoonText: "公告快捷入口尚未开放。"),
                new(HomeHotspotId.Activity, "活动", HomeHotspotState.ComingSoon, HomeHotspotTargetType.SystemShortcut,
                    comingSoonText: "活动快捷入口尚未开放。"),
                new(HomeHotspotId.Settings, "设置", HomeHotspotState.ComingSoon, HomeHotspotTargetType.SystemShortcut,
                    comingSoonText: "设置快捷入口尚未开放。")
            };
        }

        public static bool IsAllowed(HomeHotspotId hotspotId)
        {
            return hotspotId is
                HomeHotspotId.Counter or
                HomeHotspotId.Trial or
                HomeHotspotId.Refine or
                HomeHotspotId.Codex or
                HomeHotspotId.BackRoom or
                HomeHotspotId.DreamSign or
                HomeHotspotId.Explore or
                HomeHotspotId.TianjiFurnace or
                HomeHotspotId.MasterRelic or
                HomeHotspotId.PvpPlaceholder or
                HomeHotspotId.Store or
                HomeHotspotId.Mail or
                HomeHotspotId.Notice or
                HomeHotspotId.Activity or
                HomeHotspotId.Settings;
        }
    }
}
