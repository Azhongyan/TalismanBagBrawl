using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TalismanBag.V04.ChapterFlow.Chapter1
{
    public static class V04Chapter1EncounterManifest
    {
        public const string SchemaId = "V04Chapter1EncounterManifest.v1";
        public const string ChapterId = "bone_aspect_chapter_1";
        public const string ShatteredHostContentId =
            "bone_aspect_enemy_c1_01_shattered_host";
        public const string PorcelainHoundContentId =
            "bone_aspect_enemy_c1_02_porcelain_hound";
        public const string CompositionMode = "SINGLE_CONTENT_IDENTITY";
        public const string RuntimeBindingStatus =
            "IDENTITY_BOUND_RUNTIME_NOT_CONNECTED";
        public const string CoverageLabel = "DEV_ONLY_RUNTIME_COVERAGE";
        public const string ContentFinalityLabel = "NOT_CONTENT_FINAL";
        public const string BalanceFinalityLabel = "NOT_BALANCE_FINAL";
        public const string CompositionFinalityLabel =
            "NOT_FORMAL_STAGE_COMPOSITION";

        private static readonly ReadOnlyCollection<V04Chapter1EncounterBindingDefinition>
            BindingRows = new(BuildBindings());

        private static readonly V04Chapter1HeldContentDefinition HeldRow =
            new(
                "bone_aspect_enemy_c1_03_bone_swap_remnant",
                "换骨残相",
                "bline.content_binding_slot.c1.s8",
                "bline.c1.future_content_slot.bone_swap_remnant",
                "HELD_BY_BA-D3",
                "BA-D3",
                false,
                false,
                false,
                true,
                false,
                false);

        public static IReadOnlyList<V04Chapter1EncounterBindingDefinition> Bindings =>
            BindingRows;

        public static V04Chapter1HeldContentDefinition HeldContent => HeldRow;

        private static List<V04Chapter1EncounterBindingDefinition> BuildBindings()
        {
            return new List<V04Chapter1EncounterBindingDefinition>
            {
                Row(
                    "1-1",
                    "bline.content_binding_slot.c1.s1",
                    "bone_aspect_node_c1_01_ding_lane_outer",
                    ShatteredHostContentId,
                    "First-zone minimal encounter"),
                Row(
                    "1-2",
                    "bline.content_binding_slot.c1.s2",
                    "bone_aspect_node_c1_01_ding_lane_outer",
                    ShatteredHostContentId,
                    "Repeat deterministic Runtime coverage"),
                Row(
                    "1-3",
                    "bline.content_binding_slot.c1.s3",
                    "bone_aspect_node_c1_01_ding_lane_outer",
                    ShatteredHostContentId,
                    "First-zone close"),
                Row(
                    "1-4",
                    "bline.content_binding_slot.c1.s4",
                    "bone_aspect_node_c1_02_bone_shop_outer_lane",
                    PorcelainHoundContentId,
                    "Introduce shell pressure"),
                Row(
                    "1-5",
                    "bline.content_binding_slot.c1.s5",
                    "bone_aspect_node_c1_02_bone_shop_outer_lane",
                    PorcelainHoundContentId,
                    "Shell repeat"),
                Row(
                    "1-6",
                    "bline.content_binding_slot.c1.s6",
                    "bone_aspect_node_c1_02_bone_shop_outer_lane",
                    PorcelainHoundContentId,
                    "Shell repeat"),
                Row(
                    "1-7",
                    "bline.content_binding_slot.c1.s7",
                    "bone_aspect_node_c1_02_bone_shop_outer_lane",
                    PorcelainHoundContentId,
                    "Second-zone close"),
                Row(
                    "1-8",
                    "bline.content_binding_slot.c1.s8",
                    "bone_aspect_node_c1_03_bone_shop_inner_hall",
                    ShatteredHostContentId,
                    "Temporary dev substitute for held future slot",
                    "bline.c1.future_content_slot.bone_swap_remnant",
                    "HELD_BY_BA-D3",
                    "BA-D3"),
                Row(
                    "1-9",
                    "bline.content_binding_slot.c1.s9",
                    "bone_aspect_node_c1_03_bone_shop_inner_hall",
                    PorcelainHoundContentId,
                    "Boss-precheck dev encounter")
            };
        }

        private static V04Chapter1EncounterBindingDefinition Row(
            string stageId,
            string slotId,
            string encounterNodeId,
            string contentId,
            string purpose,
            string futureSlotId = "",
            string futureStatus = "NONE",
            string dependency = "NONE")
        {
            return new V04Chapter1EncounterBindingDefinition(
                SchemaId,
                ChapterId,
                stageId,
                slotId,
                encounterNodeId,
                contentId,
                purpose,
                CompositionMode,
                RuntimeBindingStatus,
                futureSlotId,
                futureStatus,
                dependency,
                true,
                false,
                false);
        }
    }
}
