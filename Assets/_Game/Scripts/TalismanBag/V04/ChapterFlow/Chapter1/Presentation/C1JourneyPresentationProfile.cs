using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    public enum C1JourneyEnemyVisualState
    {
        Idle = 0,
        BasicAttack = 1,
        Skill = 2,
        Hit = 3,
        Death = 4
    }

    public enum C1JourneyTransitionBeat
    {
        Hold = 0,
        ChapterStartToSegmentA = 1,
        SegmentAToSegmentB = 2,
        SegmentBToSegmentC = 3,
        Stage19ExitToBossGateAndReveal = 4
    }

    public sealed class C1JourneyStageBeat
    {
        public readonly int sequence;
        public readonly string stageId;
        public readonly string segmentId;
        public readonly string segmentDisplayName;
        public readonly string encounterNodeId;
        public readonly string runtimeContentId;
        public readonly string runtimeProfileId;
        public readonly string presentationContentId;
        public readonly string visualProfileKey;
        public readonly string assetRoot;
        public readonly string fallbackTag;
        public readonly C1JourneyTransitionBeat transitionBeat;
        public readonly bool bossStage;
        public readonly bool runtimeMechanicHeld;
        public readonly bool devOnly = true;
        public readonly bool isEnabled;
        public readonly bool formalFlow;

        public C1JourneyStageBeat(
            int sequence,
            string stageId,
            string segmentId,
            string segmentDisplayName,
            string encounterNodeId,
            string runtimeContentId,
            string runtimeProfileId,
            string presentationContentId,
            string visualProfileKey,
            string assetRoot,
            string fallbackTag,
            C1JourneyTransitionBeat transitionBeat,
            bool bossStage,
            bool runtimeMechanicHeld)
        {
            this.sequence = sequence;
            this.stageId = stageId ?? string.Empty;
            this.segmentId = segmentId ?? string.Empty;
            this.segmentDisplayName = segmentDisplayName ?? string.Empty;
            this.encounterNodeId = encounterNodeId ?? string.Empty;
            this.runtimeContentId = runtimeContentId ?? string.Empty;
            this.runtimeProfileId = runtimeProfileId ?? string.Empty;
            this.presentationContentId = presentationContentId ?? string.Empty;
            this.visualProfileKey = visualProfileKey ?? string.Empty;
            this.assetRoot = assetRoot ?? string.Empty;
            this.fallbackTag = fallbackTag ?? string.Empty;
            this.transitionBeat = transitionBeat;
            this.bossStage = bossStage;
            this.runtimeMechanicHeld = runtimeMechanicHeld;
            isEnabled = false;
            formalFlow = false;
        }
    }

    public static class C1JourneyPresentationProfile
    {
        public const string PackageId =
            "V0.4-C1BattleSandboxEncounterOwnershipAndVisualSlotHandoffFix01";
        public const string SchemaId = "C1JourneyPresentationProfile.v2";
        public const string TargetSceneName =
            "Scene_TalismanBag_V04_BattleSandboxPreview";
        public const string TargetScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        public const string SegmentA =
            "bone_aspect_node_c1_01_ding_lane_outer";
        public const string SegmentB =
            "bone_aspect_node_c1_02_bone_shop_outer_lane";
        public const string SegmentC =
            "bone_aspect_node_c1_03_bone_shop_inner_hall";
        public const string ThirdEnemyContentId =
            "bone_aspect_enemy_c1_03_bone_swap_remnant";
        public const string BossContentId =
            "bone_aspect_boss_c1_bone_guard";
        public const string ThirdEnemyVisualProfileKey =
            "enemy.bone_aspect.c1.bone_swap_remnant.visual_fallback.d1_3.v1";
        public const string ThirdEnemyAssetRoot =
            "anim/Enemy/d1/d1_3";
        public const string TemporaryFallbackTag =
            "TEMPORARY_VISUAL_FALLBACK_D1_3";
        public const string ThirdEnemyRuntimeStatus = "HELD_BY_BA-D3";
        public const string RuntimeRootName =
            "V04_C1_JourneyPresentation_RuntimeRoot";
        public const string PanelRootName =
            "V04_C1_JourneyPresentation_PanelRoot";
        public const string EditorPreferenceKey =
            "TalismanBag.V04.C1JourneyPresentation.Enabled";

        private static readonly ReadOnlyCollection<C1JourneyEnemyVisualState>
            EnemyStates = Array.AsReadOnly(new[]
            {
                C1JourneyEnemyVisualState.Idle,
                C1JourneyEnemyVisualState.BasicAttack,
                C1JourneyEnemyVisualState.Skill,
                C1JourneyEnemyVisualState.Hit,
                C1JourneyEnemyVisualState.Death
            });

        private static readonly ReadOnlyCollection<C1JourneyStageBeat> Rows =
            new(BuildRows());

        public static IReadOnlyList<C1JourneyEnemyVisualState>
            FiveEnemyStates => EnemyStates;

        public static IReadOnlyList<C1JourneyStageBeat> StageBeats => Rows;

        public static C1JourneyStageBeat Find(string stageId)
        {
            return Rows.FirstOrDefault(row => string.Equals(
                row.stageId,
                stageId,
                StringComparison.Ordinal));
        }

        public static string GetFallbackStateResourcePath(
            C1JourneyEnemyVisualState state)
        {
            string folder = state switch
            {
                C1JourneyEnemyVisualState.Idle => "d1_3_Idle",
                C1JourneyEnemyVisualState.BasicAttack => "d1_3_Attack",
                C1JourneyEnemyVisualState.Skill => "d1_3_Skill",
                C1JourneyEnemyVisualState.Hit => "d1_3_Hit",
                C1JourneyEnemyVisualState.Death => "d1_3_Death",
                _ => "d1_3_Idle"
            };
            return ThirdEnemyAssetRoot + "/" + folder + "/frames";
        }

        private static List<C1JourneyStageBeat> BuildRows()
        {
            List<C1JourneyStageBeat> rows = new();
            for (int stageIndex = 1; stageIndex <= 10; stageIndex++)
            {
                string stageId = "1-" + stageIndex;
                bool boss = stageIndex == 10;
                V04Chapter1EncounterBindingDefinition binding = boss
                    ? null
                    : V04Chapter1EncounterManifest.Bindings.First(
                        row => row.stageId == stageId);
                C1EnemyRuntimeProfileSnapshot runtimeProfile = binding == null
                    ? null
                    : C1EnemyRuntimeCatalog.GetProfiles().FirstOrDefault(
                        profile => profile.ContentId ==
                            binding.activeEnemyContentId);

                bool thirdEnemyPresentation =
                    stageIndex == 8 || stageIndex == 9;
                string segment = stageIndex <= 3
                    ? SegmentA
                    : stageIndex <= 7
                        ? SegmentB
                        : SegmentC;
                string segmentDisplayName = stageIndex <= 3
                    ? "Segment A · 丁巷外沿"
                    : stageIndex <= 7
                        ? "Segment B · 骨器铺外巷"
                        : "Segment C · 骨器铺内堂";
                C1JourneyTransitionBeat transition = stageIndex switch
                {
                    1 => C1JourneyTransitionBeat.ChapterStartToSegmentA,
                    4 => C1JourneyTransitionBeat.SegmentAToSegmentB,
                    8 => C1JourneyTransitionBeat.SegmentBToSegmentC,
                    10 => C1JourneyTransitionBeat
                        .Stage19ExitToBossGateAndReveal,
                    _ => C1JourneyTransitionBeat.Hold
                };

                rows.Add(new C1JourneyStageBeat(
                    stageIndex,
                    stageId,
                    segment,
                    segmentDisplayName,
                    boss ? SegmentC : binding.encounterNodeId,
                    boss ? BossContentId : binding.activeEnemyContentId,
                    runtimeProfile?.RuntimeProfileId ?? string.Empty,
                    boss
                        ? BossContentId
                        : thirdEnemyPresentation
                            ? ThirdEnemyContentId
                            : binding.activeEnemyContentId,
                    boss
                        ? "enemy.bone_aspect.c1.bone_guard.phase1"
                        : thirdEnemyPresentation
                            ? ThirdEnemyVisualProfileKey
                            : runtimeProfile?.PresentationKey ?? string.Empty,
                    thirdEnemyPresentation
                        ? ThirdEnemyAssetRoot
                        : string.Empty,
                    thirdEnemyPresentation
                        ? TemporaryFallbackTag
                        : string.Empty,
                    transition,
                    boss,
                    thirdEnemyPresentation));
            }
            return rows;
        }
    }
}
