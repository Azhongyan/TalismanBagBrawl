using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TalismanBag.V04.ChapterFlow
{
    public static class V04ChapterFlowManifest
    {
        public const string SchemaId = "V04ChapterFlowManifest.v1";
        public const string SourceRouteId = "v04_bline_chapter_flow_devonly";

        private static readonly string[] ChapterIds =
        {
            "bone_aspect_chapter_1",
            "bone_aspect_chapter_2",
            "bone_aspect_chapter_3",
            "bone_aspect_chapter_4"
        };

        private static readonly string[] BossProfileIds =
        {
            "bone_aspect_boss_c1_bone_guard",
            "bone_aspect_boss_c2_identity_bidder",
            "bone_aspect_boss_c3_array_eye_guardian",
            "bone_aspect_boss_c4_myriad_bone_beast"
        };

        private static readonly EncounterRange[] EncounterRanges =
        {
            new EncounterRange(1, 1, 3, "bone_aspect_node_c1_01_ding_lane_outer"),
            new EncounterRange(1, 4, 7, "bone_aspect_node_c1_02_bone_shop_outer_lane"),
            new EncounterRange(1, 8, 10, "bone_aspect_node_c1_03_bone_shop_inner_hall"),
            new EncounterRange(2, 1, 3, "bone_aspect_node_c2_01_parking_entrance"),
            new EncounterRange(2, 4, 7, "bone_aspect_node_c2_02_rakshasa_market_street"),
            new EncounterRange(2, 8, 10, "bone_aspect_node_c2_03_bone_auction_hall"),
            new EncounterRange(3, 1, 3, "bone_aspect_node_c3_01_abandoned_mountain_path"),
            new EncounterRange(3, 4, 7, "bone_aspect_node_c3_02_collapsed_mountain_gate"),
            new EncounterRange(3, 8, 10, "bone_aspect_node_c3_03_memory_hall_array_eye"),
            new EncounterRange(4, 1, 3, "bone_aspect_node_c4_01_sealed_ding_lane_defense"),
            new EncounterRange(4, 4, 7, "bone_aspect_node_c4_02_transformed_bone_shop"),
            new EncounterRange(4, 8, 10, "bone_aspect_node_c4_03_myriad_bone_belly")
        };

        private static readonly ReadOnlyCollection<V04ChapterDefinition> ChapterRows =
            new(BuildChapters());

        private static readonly ReadOnlyCollection<V04ChapterStageDefinition> StageRows =
            new(BuildStages());

        public static IReadOnlyList<V04ChapterDefinition> Chapters => ChapterRows;

        public static IReadOnlyList<V04ChapterStageDefinition> Stages => StageRows;

        public static V04ChapterDefinition FindChapter(string chapterId)
        {
            string normalized = Normalize(chapterId);
            return ChapterRows.FirstOrDefault(
                row => string.Equals(row.chapterId, normalized, StringComparison.Ordinal));
        }

        public static V04ChapterStageDefinition FindStage(string stageId)
        {
            string normalized = Normalize(stageId);
            return StageRows.FirstOrDefault(
                row => string.Equals(row.stageId, normalized, StringComparison.Ordinal));
        }

        public static V04ChapterStageDefinition FindFirstIncompleteStage(
            string chapterId,
            IEnumerable<string> completedStageIds)
        {
            HashSet<string> completed = new(
                completedStageIds ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            return StageRows
                .Where(row => string.Equals(row.chapterId, chapterId, StringComparison.Ordinal))
                .OrderBy(row => row.stageIndex)
                .FirstOrDefault(row => !completed.Contains(row.stageId));
        }

        public static V04ChapterStageDefinition FindNextStage(V04ChapterStageDefinition stage)
        {
            if (stage == null || stage.stageIndex >= 10)
            {
                return null;
            }

            return FindStage(stage.chapterIndex + "-" + (stage.stageIndex + 1));
        }

        public static V04ChapterDefinition FindNextChapter(V04ChapterDefinition chapter)
        {
            if (chapter == null || chapter.chapterIndex >= ChapterRows.Count)
            {
                return null;
            }

            return ChapterRows.FirstOrDefault(row => row.chapterIndex == chapter.chapterIndex + 1);
        }

        private static List<V04ChapterDefinition> BuildChapters()
        {
            List<V04ChapterDefinition> rows = new();
            for (int index = 0; index < ChapterIds.Length; index++)
            {
                int chapterIndex = index + 1;
                rows.Add(new V04ChapterDefinition(
                    ChapterIds[index],
                    chapterIndex,
                    chapterIndex + "-1",
                    chapterIndex + "-10",
                    BossProfileIds[index]));
            }

            return rows;
        }

        private static List<V04ChapterStageDefinition> BuildStages()
        {
            List<V04ChapterStageDefinition> rows = new();
            for (int chapterIndex = 1; chapterIndex <= 4; chapterIndex++)
            {
                string chapterId = ChapterIds[chapterIndex - 1];
                for (int stageIndex = 1; stageIndex <= 10; stageIndex++)
                {
                    string stageId = chapterIndex + "-" + stageIndex;
                    bool boss = stageIndex == 10;
                    bool bossGate = stageIndex == 9;
                    string identity = "c" + chapterIndex + ".s" + stageIndex;
                    string encounterNodeId = ResolveEncounterNodeId(chapterIndex, stageIndex);
                    rows.Add(new V04ChapterStageDefinition(
                        chapterId,
                        chapterIndex,
                        stageId,
                        stageIndex,
                        encounterNodeId,
                        boss,
                        bossGate,
                        boss,
                        boss ? string.Empty : "bline.content_binding_slot." + identity,
                        boss ? BossProfileIds[chapterIndex - 1] : string.Empty,
                        boss
                            ? (chapterIndex == 3
                                ? V04BossResolutionMode.YieldAndAllowPassage
                                : V04BossResolutionMode.RouteCompletion)
                            : V04BossResolutionMode.None,
                        stageIndex == 1 ? Hook("before_chapter", identity, "tutorial") : string.Empty,
                        stageIndex == 1 ? Hook("before_chapter", identity, "story") : string.Empty,
                        Hook("before_stage", identity, "tutorial"),
                        Hook("before_stage", identity, "story"),
                        Hook("after_result", identity, "tutorial"),
                        Hook("after_result", identity, "story"),
                        bossGate ? Hook("before_boss_gate", identity, "tutorial") : string.Empty,
                        bossGate ? Hook("before_boss_gate", identity, "story") : string.Empty,
                        boss ? Hook("before_boss_challenge", identity, "tutorial") : string.Empty,
                        boss ? Hook("before_boss_challenge", identity, "story") : string.Empty,
                        boss ? Hook("after_boss_result", identity, "tutorial") : string.Empty,
                        boss ? Hook("after_boss_result", identity, "story") : string.Empty,
                        boss ? Hook("before_next_chapter_unlock", identity, "tutorial") : string.Empty,
                        boss ? Hook("before_next_chapter_unlock", identity, "story") : string.Empty,
                        boss ? string.Empty : Hook("drop_candidate_preview", identity, "contract")));
                }
            }

            return rows;
        }

        private static string ResolveEncounterNodeId(int chapterIndex, int stageIndex)
        {
            EncounterRange range = EncounterRanges.FirstOrDefault(
                candidate => candidate.ChapterIndex == chapterIndex
                    && stageIndex >= candidate.StartStage
                    && stageIndex <= candidate.EndStage);
            return range == null ? string.Empty : range.NodeId;
        }

        private static string Hook(string category, string identity, string channel)
        {
            return "bline.chapter_flow." + category + "." + identity + "." + channel;
        }

        private static string Normalize(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private sealed class EncounterRange
        {
            public readonly int ChapterIndex;
            public readonly int StartStage;
            public readonly int EndStage;
            public readonly string NodeId;

            public EncounterRange(int chapterIndex, int startStage, int endStage, string nodeId)
            {
                ChapterIndex = chapterIndex;
                StartStage = startStage;
                EndStage = endStage;
                NodeId = nodeId;
            }
        }
    }
}
