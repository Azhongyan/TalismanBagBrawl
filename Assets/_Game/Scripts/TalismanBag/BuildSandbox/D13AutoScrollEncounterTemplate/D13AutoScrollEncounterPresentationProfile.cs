using System;
using UnityEngine;

namespace TalismanBag.BuildSandbox.D13AutoScrollEncounterTemplate
{
    internal enum D13AutoScrollAnimationSlot
    {
        Idle,
        Attack,
        Skill,
        HeavyHit,
        Death
    }

    internal sealed class D13AutoScrollSequenceProfile
    {
        public D13AutoScrollSequenceProfile(
            D13AutoScrollAnimationSlot slot,
            string resourcePath,
            float frameSeconds,
            Vector2 visualSize,
            Vector2 footLineOffset)
        {
            Slot = slot;
            ResourcePath = resourcePath ?? string.Empty;
            FrameSeconds = Mathf.Max(0.01f, frameSeconds);
            VisualSize = visualSize;
            FootLineOffset = footLineOffset;
        }

        public D13AutoScrollAnimationSlot Slot { get; }

        public string ResourcePath { get; }

        public float FrameSeconds { get; }

        public Vector2 VisualSize { get; }

        public Vector2 FootLineOffset { get; }
    }

    internal sealed class D13AutoScrollEncounterPresentationProfile
    {
        public const string TargetSceneName =
            "Scene_TalismanBag_V04_BattleSandboxPreview";

        public const string AuthoredPlayerImageName = "player";

        public const string AuthoredStageName = "V02AutoCombatStage";

        private static readonly D13AutoScrollSequenceProfile[] EnemySequences =
        {
            new(
                D13AutoScrollAnimationSlot.Idle,
                "anim/Enemy/d1/d1_3/d1_3_Idle/frames",
                0.10f,
                new Vector2(254f, 254f),
                new Vector2(0f, -12f)),
            new(
                D13AutoScrollAnimationSlot.Attack,
                "anim/Enemy/d1/d1_3/d1_3_Attack/frames",
                0.10f,
                new Vector2(266f, 266f),
                new Vector2(0f, -12f)),
            new(
                D13AutoScrollAnimationSlot.Skill,
                "anim/Enemy/d1/d1_3/d1_3_Skill/frames",
                0.10f,
                new Vector2(306f, 306f),
                new Vector2(1f, -14f)),
            new(
                D13AutoScrollAnimationSlot.HeavyHit,
                "anim/Enemy/d1/d1_3/d1_3_Hit/frames",
                0.10f,
                new Vector2(306f, 306f),
                new Vector2(-1f, -14f)),
            new(
                D13AutoScrollAnimationSlot.Death,
                "anim/Enemy/d1/d1_3/d1_3_Death/frames",
                0.10f,
                new Vector2(266f, 266f),
                new Vector2(0f, -12f))
        };

        public static D13AutoScrollEncounterPresentationProfile Default { get; } =
            new();

        private D13AutoScrollEncounterPresentationProfile()
        {
        }

        public float ApproachSeconds => 2.35f;

        public float ReadySeconds => 0.85f;

        public float InterActionHoldSeconds => 0.42f;

        public float PlayerStrikeSeconds => 0.58f;

        public float LootBeatSeconds => 0.95f;

        public float ExitSeconds => 1.75f;

        public float ReplayDelaySeconds => 0.35f;

        public float RunBobAmplitude => 5.5f;

        public float RunLeanDegrees => 7f;

        public float DustStepInterval => 0.24f;

        public float OffscreenMargin => 155f;

        public float PlayerAnchorFactor => -0.43f;

        public float EnemyAnchorFactor => 0.43f;

        public float NormalHitSeconds => 0.24f;

        public float FloatTextSeconds => 0.72f;

        public D13AutoScrollSequenceProfile[] CreateEnemySequences()
        {
            return (D13AutoScrollSequenceProfile[])EnemySequences.Clone();
        }
    }
}
