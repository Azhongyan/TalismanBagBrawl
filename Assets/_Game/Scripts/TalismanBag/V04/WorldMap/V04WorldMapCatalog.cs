using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.V04.ChapterFlow;

namespace TalismanBag.V04.WorldMap
{
    public enum V04WorldMapStageVisualState
    {
        Locked = 0,
        Available = 1,
        Cleared = 2,
        Boss = 3
    }

    public sealed class V04WorldMapDropPreviewDefinition
    {
        public readonly string slotId;
        public readonly string displayName;
        public readonly string bindingStatus;

        public V04WorldMapDropPreviewDefinition(
            string slotId,
            string displayName,
            string bindingStatus)
        {
            this.slotId = slotId;
            this.displayName = displayName;
            this.bindingStatus = bindingStatus;
        }
    }

    public sealed class V04WorldMapStageVisualStateDefinition
    {
        public readonly V04WorldMapStageVisualState state;
        public readonly string displayName;
        public readonly string styleKey;
        public readonly string badgeColorHtml;

        public V04WorldMapStageVisualStateDefinition(
            V04WorldMapStageVisualState state,
            string displayName,
            string styleKey,
            string badgeColorHtml)
        {
            this.state = state;
            this.displayName = displayName;
            this.styleKey = styleKey;
            this.badgeColorHtml = badgeColorHtml;
        }
    }

    public sealed class V04WorldMapStageDefinition
    {
        public readonly string stageId;
        public readonly string chapterId;
        public readonly int stageIndex;
        public readonly string encounterNodeId;
        public readonly string displayName;
        public readonly string locationDisplayName;
        public readonly string enemySummary;
        public readonly string rankRangeLabel;
        public readonly string firstClearRewardLabel;
        public readonly bool isBossStage;
        public readonly string bossProfileId;
        public readonly IReadOnlyList<V04WorldMapDropPreviewDefinition> dropPreviewRows;

        public V04WorldMapStageDefinition(
            string stageId,
            string chapterId,
            int stageIndex,
            string encounterNodeId,
            string displayName,
            string locationDisplayName,
            string enemySummary,
            string rankRangeLabel,
            string firstClearRewardLabel,
            bool isBossStage,
            string bossProfileId,
            IReadOnlyList<V04WorldMapDropPreviewDefinition> dropPreviewRows)
        {
            this.stageId = stageId;
            this.chapterId = chapterId;
            this.stageIndex = stageIndex;
            this.encounterNodeId = encounterNodeId;
            this.displayName = displayName;
            this.locationDisplayName = locationDisplayName;
            this.enemySummary = enemySummary;
            this.rankRangeLabel = rankRangeLabel;
            this.firstClearRewardLabel = firstClearRewardLabel;
            this.isBossStage = isBossStage;
            this.bossProfileId = bossProfileId;
            this.dropPreviewRows = dropPreviewRows;
        }
    }

    public sealed class V04WorldMapChapterDefinition
    {
        public readonly string chapterId;
        public readonly int chapterIndex;
        public readonly string displayName;
        public readonly string subtitle;
        public readonly string visualFamilyId;
        public readonly bool availableInVerticalSlice;

        public V04WorldMapChapterDefinition(
            string chapterId,
            int chapterIndex,
            string displayName,
            string subtitle,
            string visualFamilyId,
            bool availableInVerticalSlice)
        {
            this.chapterId = chapterId;
            this.chapterIndex = chapterIndex;
            this.displayName = displayName;
            this.subtitle = subtitle;
            this.visualFamilyId = visualFamilyId;
            this.availableInVerticalSlice = availableInVerticalSlice;
        }
    }

    public sealed class V04WorldMapRegionDefinition
    {
        public readonly string regionId;
        public readonly string displayName;
        public readonly string versionThemeId;
        public readonly IReadOnlyList<V04WorldMapChapterDefinition> chapters;

        public V04WorldMapRegionDefinition(
            string regionId,
            string displayName,
            string versionThemeId,
            IReadOnlyList<V04WorldMapChapterDefinition> chapters)
        {
            this.regionId = regionId;
            this.displayName = displayName;
            this.versionThemeId = versionThemeId;
            this.chapters = chapters;
        }
    }

    public sealed class V04WorldMapDefinition
    {
        public readonly string worldId;
        public readonly string displayName;
        public readonly IReadOnlyList<V04WorldMapRegionDefinition> regions;

        public V04WorldMapDefinition(
            string worldId,
            string displayName,
            IReadOnlyList<V04WorldMapRegionDefinition> regions)
        {
            this.worldId = worldId;
            this.displayName = displayName;
            this.regions = regions;
        }
    }

    public static class V04WorldMapCatalog
    {
        public const string SchemaId = "V04WorldMapCatalog.v1";
        public const string WorldId = "minglushanxia_world";
        public const string QingshifangRegionId = "qingshifang_bone_aspect";
        public const string QingshifangThemeId = "bone_aspect";
        public const string ChapterOneId = "bone_aspect_chapter_1";
        public const string DefaultAvailableStageId = "1-1";
        public const string UnboundDropStatus = "DROP_TABLE_OWNER_UNBOUND";
        public const string UnboundRankStatus = "RANK_OWNER_UNBOUND";
        public const string UnboundRewardStatus = "FIRST_CLEAR_REWARD_OWNER_UNBOUND";

        private static readonly ReadOnlyCollection<V04WorldMapStageVisualStateDefinition>
            StageVisualStateRows = new(
                new List<V04WorldMapStageVisualStateDefinition>
                {
                    new(
                        V04WorldMapStageVisualState.Locked,
                        "未解锁",
                        "world_map.stage.locked",
                        "#5B5B62"),
                    new(
                        V04WorldMapStageVisualState.Available,
                        "可挑战",
                        "world_map.stage.available",
                        "#B8893E"),
                    new(
                        V04WorldMapStageVisualState.Cleared,
                        "已通关",
                        "world_map.stage.cleared",
                        "#4F8B62"),
                    new(
                        V04WorldMapStageVisualState.Boss,
                        "首领",
                        "world_map.stage.boss",
                        "#9B3A35")
                });

        private static readonly ReadOnlyCollection<V04WorldMapChapterDefinition> ChapterRows =
            new(
                new List<V04WorldMapChapterDefinition>
                {
                    new(
                        "bone_aspect_chapter_1",
                        1,
                        "第一章·骨瓷与旧街",
                        "青石坊老街第一次露出骨相异变",
                        "bone_aspect_visual_c1_bone_porcelain",
                        true),
                    new(
                        "bone_aspect_chapter_2",
                        2,
                        "第二章·契纸与错误身份",
                        "身份、才华和命运被交易与定价",
                        "bone_aspect_visual_c2_contract_identity",
                        false),
                    new(
                        "bone_aspect_chapter_3",
                        3,
                        "第三章·阵石与记忆叠影",
                        "明箓山旧阵、资格判断与残缺记忆",
                        "bone_aspect_visual_c3_array_stone_memory",
                        false),
                    new(
                        "bone_aspect_chapter_4",
                        4,
                        "第四章·赝骨与复合法相",
                        "被夺身份汇聚为不可定价的终局冲突",
                        "bone_aspect_visual_c4_counterfeit_composite",
                        false)
                });

        private static readonly ReadOnlyCollection<V04WorldMapStageDefinition> ChapterOneStageRows =
            new(BuildChapterOneStages());

        private static readonly V04WorldMapRegionDefinition QingshifangRegion =
            new(
                QingshifangRegionId,
                "青石坊《骨相》",
                QingshifangThemeId,
                ChapterRows);

        private static readonly V04WorldMapDefinition WorldRow =
            new(
                WorldId,
                "《明箓山下》世界舆图",
                new ReadOnlyCollection<V04WorldMapRegionDefinition>(
                    new List<V04WorldMapRegionDefinition> { QingshifangRegion }));

        public static V04WorldMapDefinition World => WorldRow;

        public static V04WorldMapRegionDefinition Qingshifang => QingshifangRegion;

        public static IReadOnlyList<V04WorldMapChapterDefinition> Chapters => ChapterRows;

        public static IReadOnlyList<V04WorldMapStageDefinition> ChapterOneStages =>
            ChapterOneStageRows;

        public static IReadOnlyList<V04WorldMapStageVisualStateDefinition> StageVisualStates =>
            StageVisualStateRows;

        public static V04WorldMapChapterDefinition FindChapter(string chapterId)
        {
            string normalized = Normalize(chapterId);
            return ChapterRows.FirstOrDefault(
                row => string.Equals(row.chapterId, normalized, StringComparison.Ordinal));
        }

        public static V04WorldMapStageDefinition FindChapterOneStage(string stageId)
        {
            string normalized = Normalize(stageId);
            return ChapterOneStageRows.FirstOrDefault(
                row => string.Equals(row.stageId, normalized, StringComparison.Ordinal));
        }

        public static V04WorldMapStageVisualStateDefinition FindVisualState(
            V04WorldMapStageVisualState state)
        {
            return StageVisualStateRows.First(row => row.state == state);
        }

        private static List<V04WorldMapStageDefinition> BuildChapterOneStages()
        {
            List<V04WorldMapStageDefinition> rows = new();
            for (int stageIndex = 1; stageIndex <= 10; stageIndex++)
            {
                string stageId = "1-" + stageIndex;
                V04ChapterStageDefinition chapterFlowStage =
                    V04ChapterFlowManifest.FindStage(stageId);
                if (chapterFlowStage == null
                    || !string.Equals(
                        chapterFlowStage.chapterId,
                        ChapterOneId,
                        StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        "WORLD_MAP_CHAPTER_FLOW_STAGE_IDENTITY_MISSING stageId=" + stageId);
                }

                string locationName = ResolveChapterOneLocation(stageIndex);
                bool boss = chapterFlowStage.isBossStage;
                string enemySummary = boss
                    ? "首领身份：守骨奴\n仅展示稳定身份；战斗数据由 Battle / Enemy Owner 后续绑定"
                    : "遭遇地：" + locationName +
                      "\n敌人编排由 Battle / Enemy Owner 后续绑定";

                rows.Add(
                    new V04WorldMapStageDefinition(
                        stageId,
                        chapterFlowStage.chapterId,
                        stageIndex,
                        chapterFlowStage.encounterNodeId,
                        stageId + " · " + locationName,
                        locationName,
                        enemySummary,
                        "品阶范围：待数值 Owner 配置（" + UnboundRankStatus + "）",
                        "首通奖励：待 Reward Owner 配置（" + UnboundRewardStatus + "）",
                        boss,
                        chapterFlowStage.bossProfileId,
                        BuildDropPreviewRows(stageId)));
            }

            return rows;
        }

        private static IReadOnlyList<V04WorldMapDropPreviewDefinition> BuildDropPreviewRows(
            string stageId)
        {
            return new ReadOnlyCollection<V04WorldMapDropPreviewDefinition>(
                new List<V04WorldMapDropPreviewDefinition>
                {
                    new(
                        "world_map.drop_preview." + stageId + ".primary",
                        "定向掉落槽位 A",
                        UnboundDropStatus),
                    new(
                        "world_map.drop_preview." + stageId + ".secondary",
                        "定向掉落槽位 B",
                        UnboundDropStatus),
                    new(
                        "world_map.drop_preview." + stageId + ".rare",
                        "稀有掉落槽位",
                        UnboundDropStatus)
                });
        }

        private static string ResolveChapterOneLocation(int stageIndex)
        {
            if (stageIndex <= 3)
            {
                return "青石坊丁巷外街";
            }

            if (stageIndex <= 7)
            {
                return "骨铺外巷";
            }

            return "骨铺内堂";
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }
    }
}
