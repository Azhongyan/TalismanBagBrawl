using System;
using System.Collections.Generic;
using TalismanBag.EnemySystem.BoneAspect.C1EnemyRuntime;
using UnityEngine;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    public sealed class C1EnemyVisualClipDefinition
    {
        public C1EnemyVisualClipDefinition(
            C1EnemyPresentationVisualState state,
            string resourcePath,
            bool frameSequence,
            int frameDurationMilliseconds,
            bool loops,
            C1EnemyVisualCalibration calibration,
            params int[] selectedFrameOrdinals)
        {
            State = state;
            ResourcePath = resourcePath ?? string.Empty;
            FrameSequence = frameSequence;
            FrameDurationMilliseconds =
                Mathf.Max(1, frameDurationMilliseconds);
            Loops = loops;
            Calibration = calibration;
            SelectedFrameOrdinals =
                selectedFrameOrdinals ?? Array.Empty<int>();
        }

        public C1EnemyPresentationVisualState State { get; }
        public string ResourcePath { get; }
        public bool FrameSequence { get; }
        public int FrameDurationMilliseconds { get; }
        public bool Loops { get; }
        public C1EnemyVisualCalibration Calibration { get; }
        public IReadOnlyList<int> SelectedFrameOrdinals { get; }
    }

    public sealed class C1EnemyVisualProfileDefinition
    {
        private readonly Dictionary<
            C1EnemyPresentationVisualState,
            C1EnemyVisualClipDefinition> clips;

        public C1EnemyVisualProfileDefinition(
            string visualProfileKey,
            bool runtimeRenderable,
            string blockerCode,
            string temporaryFallbackResourceRoot,
            params C1EnemyVisualClipDefinition[] clips)
        {
            VisualProfileKey = visualProfileKey ?? string.Empty;
            RuntimeRenderable = runtimeRenderable;
            BlockerCode = blockerCode ?? string.Empty;
            TemporaryFallbackResourceRoot =
                temporaryFallbackResourceRoot ?? string.Empty;
            this.clips = new Dictionary<
                C1EnemyPresentationVisualState,
                C1EnemyVisualClipDefinition>();
            foreach (C1EnemyVisualClipDefinition clip
                     in clips ?? Array.Empty<
                         C1EnemyVisualClipDefinition>())
            {
                this.clips[clip.State] = clip;
            }
        }

        public string VisualProfileKey { get; }
        public bool RuntimeRenderable { get; }
        public string BlockerCode { get; }
        public string TemporaryFallbackResourceRoot { get; }

        public bool TryGetClip(
            C1EnemyPresentationVisualState state,
            out C1EnemyVisualClipDefinition clip)
        {
            if (clips.TryGetValue(state, out clip))
            {
                return true;
            }
            return clips.TryGetValue(
                C1EnemyPresentationVisualState.Idle,
                out clip);
        }
    }

    public static class C1EnemyVisualProfileCatalog
    {
        public const string TemporaryRemnantFallbackTag =
            "TEMPORARY_VISUAL_FALLBACK_D1_3";

        private static readonly RectInt HostReferenceBounds =
            new(0, 0, 960, 960);
        private static readonly RectInt HostUnionBounds =
            new(14, 0, 945, 949);
        private static readonly RectInt HostLargeStateBounds =
            new(0, 0, 1440, 1440);
        private static readonly RectInt HoundReferenceBounds =
            new(0, 0, 512, 765);

        private static readonly IReadOnlyDictionary<
            string,
            C1EnemyVisualProfileDefinition> Profiles =
                BuildProfiles();

        public static bool TryGet(
            string visualProfileKey,
            out C1EnemyVisualProfileDefinition profile)
        {
            return Profiles.TryGetValue(
                visualProfileKey ?? string.Empty,
                out profile);
        }

        private static IReadOnlyDictionary<
            string,
            C1EnemyVisualProfileDefinition> BuildProfiles()
        {
            C1EnemyVisualCalibration hostCalibration =
                C1EnemyVisualCalibration.Identity(
                    HostReferenceBounds,
                    HostUnionBounds);
            C1EnemyVisualCalibration hostLargeStateCalibration =
                C1EnemyVisualCalibration.Identity(
                    HostReferenceBounds,
                    HostLargeStateBounds);
            C1EnemyVisualCalibration houndCalibration =
                C1EnemyVisualCalibration.Identity(
                    HoundReferenceBounds,
                    HoundReferenceBounds);

            return new Dictionary<
                string,
                C1EnemyVisualProfileDefinition>(
                StringComparer.Ordinal)
            {
                [C1EnemyRuntimeContract.ShatteredHostPresentationKey] =
                    new C1EnemyVisualProfileDefinition(
                        C1EnemyRuntimeContract
                            .ShatteredHostPresentationKey,
                        true,
                        string.Empty,
                        string.Empty,
                        Sequence(
                            C1EnemyPresentationVisualState.Idle,
                            "anim/Enemy/d1/d1_3/d1_3_Idle/frames",
                            125,
                            true,
                            hostCalibration,
                            1, 4, 7, 10, 13, 16, 19, 22),
                        Sequence(
                            C1EnemyPresentationVisualState.BasicAttack,
                            "anim/Enemy/d1/d1_3/d1_3_Attack/frames",
                            95,
                            false,
                            hostCalibration,
                            1, 4, 7, 10, 13, 17, 21, 25),
                        Sequence(
                            C1EnemyPresentationVisualState.Skill,
                            "anim/Enemy/d1/d1_3/d1_3_Skill/frames",
                            100,
                            false,
                            hostLargeStateCalibration,
                            1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 22, 25),
                        Sequence(
                            C1EnemyPresentationVisualState.Hit,
                            "anim/Enemy/d1/d1_3/d1_3_Hit/frames",
                            60,
                            false,
                            hostLargeStateCalibration,
                            1, 7, 13, 19),
                        Sequence(
                            C1EnemyPresentationVisualState.Death,
                            "anim/Enemy/d1/d1_3/d1_3_Death/frames",
                            100,
                            false,
                            hostCalibration,
                            1, 3, 5, 8, 11, 14, 17, 20, 22, 24)),
                [C1EnemyRuntimeContract.PorcelainHoundPresentationKey] =
                    new C1EnemyVisualProfileDefinition(
                        C1EnemyRuntimeContract
                            .PorcelainHoundPresentationKey,
                        true,
                        string.Empty,
                        string.Empty,
                        Single(
                            C1EnemyPresentationVisualState.Idle,
                            "Enemy/guciquan_idle",
                            true,
                            houndCalibration),
                        Single(
                            C1EnemyPresentationVisualState.BasicAttack,
                            "Enemy/guciquan_attack",
                            false,
                            houndCalibration),
                        Single(
                            C1EnemyPresentationVisualState.Skill,
                            "Enemy/guciquan_attack",
                            false,
                            houndCalibration),
                        Single(
                            C1EnemyPresentationVisualState.Hit,
                            "Enemy/guciquan_hit",
                            false,
                            houndCalibration),
                        Single(
                            C1EnemyPresentationVisualState.Death,
                            "Enemy/guciquan_death",
                            false,
                            houndCalibration)),
                ["enemy.bone_aspect.c1.bone_swap_remnant"] =
                    new C1EnemyVisualProfileDefinition(
                        "enemy.bone_aspect.c1.bone_swap_remnant",
                        false,
                        C1EnemyRuntimeContract
                            .BoneSwapRemnantHoldStatus,
                        "anim/Enemy/d1/d1_3")
            };
        }

        private static C1EnemyVisualClipDefinition Sequence(
            C1EnemyPresentationVisualState state,
            string resourcePath,
            int frameDurationMilliseconds,
            bool loops,
            C1EnemyVisualCalibration calibration,
            params int[] selectedFrameOrdinals)
        {
            return new C1EnemyVisualClipDefinition(
                state,
                resourcePath,
                true,
                frameDurationMilliseconds,
                loops,
                calibration,
                selectedFrameOrdinals);
        }

        private static C1EnemyVisualClipDefinition Single(
            C1EnemyPresentationVisualState state,
            string resourcePath,
            bool loops,
            C1EnemyVisualCalibration calibration)
        {
            return new C1EnemyVisualClipDefinition(
                state,
                resourcePath,
                false,
                350,
                loops,
                calibration);
        }
    }
}
