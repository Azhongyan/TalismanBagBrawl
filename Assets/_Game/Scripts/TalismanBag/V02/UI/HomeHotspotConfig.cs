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
                new(HomeHotspotId.Ledger, "照灯账本", HomeHotspotState.Available, HomeHotspotTargetType.CurrentObjective,
                    visualKey: "home_ledger_slot", iconKey: "icon_ledger"),
                new(HomeHotspotId.CodexBook, "道藏典册", HomeHotspotState.Available, HomeHotspotTargetType.Collection,
                    visualKey: "home_codex_book_slot", iconKey: "icon_codex"),
                new(HomeHotspotId.ClueBook, "旧物线索簿", HomeHotspotState.Locked, HomeHotspotTargetType.StoryAnchor,
                    visualKey: "home_clue_book_slot", iconKey: "icon_clue",
                    lockedReason: "线索簿暂未展开，后续记录师父旧物。"),
                new(HomeHotspotId.BackRoom, "后屋", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    visualKey: "home_back_room_slot", iconKey: "icon_back_room",
                    comingSoonText: "后屋尚未开放，后续用于生产养成。"),
                new(HomeHotspotId.DreamSign, "梦签", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    visualKey: "home_dream_sign_slot", iconKey: "icon_dream_sign",
                    comingSoonText: "梦签尚未开放，后续承接每日签。"),
                new(HomeHotspotId.Xiaoman, "小满", HomeHotspotState.Available, HomeHotspotTargetType.CharacterPrompt,
                    visualKey: "home_xiaoman_slot", iconKey: "icon_xiaoman"),
                new(HomeHotspotId.StreetEntrance, "青石坊街口", HomeHotspotState.ComingSoon, HomeHotspotTargetType.ComingSoon,
                    visualKey: "home_street_entrance_slot", iconKey: "icon_street",
                    comingSoonText: "街口暂未开放，后续承接探索与外出。"),
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
                HomeHotspotId.Ledger or
                HomeHotspotId.CodexBook or
                HomeHotspotId.ClueBook or
                HomeHotspotId.Trial or
                HomeHotspotId.Refine or
                HomeHotspotId.Codex or
                HomeHotspotId.BackRoom or
                HomeHotspotId.DreamSign or
                HomeHotspotId.Explore or
                HomeHotspotId.Xiaoman or
                HomeHotspotId.StreetEntrance or
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
