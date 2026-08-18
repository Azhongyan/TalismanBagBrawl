using System;
using UnityEngine;

namespace TalismanBag.V04.ChapterFlow.Chapter1.Presentation
{
    public enum C1EnemyPresentationVisualState
    {
        Idle = 0,
        BasicAttack = 1,
        Skill = 2,
        Hit = 3,
        Death = 4
    }

    [Serializable]
    public readonly struct C1EnemyVisualCalibration
    {
        public C1EnemyVisualCalibration(
            RectInt referencePixelBounds,
            RectInt unionPixelBounds,
            float stateScaleMultiplier,
            Vector2 normalizedLocalOffset,
            Vector2 normalizedPivot)
        {
            ReferencePixelBounds = referencePixelBounds;
            UnionPixelBounds = unionPixelBounds;
            StateScaleMultiplier = stateScaleMultiplier;
            NormalizedLocalOffset = normalizedLocalOffset;
            NormalizedPivot = normalizedPivot;
        }

        public RectInt ReferencePixelBounds { get; }
        public RectInt UnionPixelBounds { get; }
        public float StateScaleMultiplier { get; }
        public Vector2 NormalizedLocalOffset { get; }
        public Vector2 NormalizedPivot { get; }

        public static C1EnemyVisualCalibration Identity(
            RectInt referencePixelBounds,
            RectInt unionPixelBounds)
        {
            return new C1EnemyVisualCalibration(
                referencePixelBounds,
                unionPixelBounds,
                1f,
                Vector2.zero,
                new Vector2(0.5f, 0.5f));
        }
    }

    public sealed class C1EnemyPresentationActorFrame
    {
        public C1EnemyPresentationActorFrame(
            string enemyInstanceId,
            string displaySlotId,
            string visualProfileKey,
            C1EnemyPresentationVisualState visualState,
            Sprite sprite,
            C1EnemyVisualCalibration calibration,
            bool visible,
            bool targetable,
            bool selected)
        {
            EnemyInstanceId = enemyInstanceId ?? string.Empty;
            DisplaySlotId = displaySlotId ?? string.Empty;
            VisualProfileKey = visualProfileKey ?? string.Empty;
            VisualState = visualState;
            Sprite = sprite;
            Calibration = calibration;
            Visible = visible;
            Targetable = targetable;
            Selected = selected;
        }

        public string EnemyInstanceId { get; }
        public string DisplaySlotId { get; }
        public string VisualProfileKey { get; }
        public C1EnemyPresentationVisualState VisualState { get; }
        public Sprite Sprite { get; }
        public C1EnemyVisualCalibration Calibration { get; }
        public bool Visible { get; }
        public bool Targetable { get; }
        public bool Selected { get; }
        public string BindingKey =>
            EnemyInstanceId + "@" + DisplaySlotId;
    }

    public interface IC1AuthoredEnemyPresentationSlot
    {
        string DisplaySlotId { get; }
        string BoundEnemyInstanceId { get; }
        bool IsRuntimeVisible { get; }
    }
}
