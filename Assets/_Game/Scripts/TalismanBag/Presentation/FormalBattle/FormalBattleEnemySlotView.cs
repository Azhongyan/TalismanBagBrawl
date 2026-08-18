using System;
using System.Collections.Generic;
using TMPro;
using TalismanBag.Contracts.Battle;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.Presentation.FormalBattle
{
    [DisallowMultipleComponent]
    public sealed class FormalBattleEnemySlotView : MonoBehaviour
    {
        [SerializeField, Min(0)] private int authoredStableOrder;
        [SerializeField] private Image visualImage;
        [SerializeField] private RectTransform visualRect;
        [SerializeField] private C1AuthoredEnemySelectionOutlineEffect
            targetOutline;
        [SerializeField] private C1AuthoredEnemyCalibrationMeshEffect
            calibrationEffect;
        [SerializeField] private CanvasGroup telegraphGroup;
        [SerializeField] private Image hitOverlayImage;
        [SerializeField] private RectTransform damageAnchor;
        [SerializeField] private RectTransform effectFloatAnchor;
        [SerializeField] private FormalBattleFeedbackReceiverLanes
            feedbackReceiverLanes;

        private readonly Dictionary<Texture2D, Sprite> runtimeSprites =
            new Dictionary<Texture2D, Sprite>();

        private FormalBattleEnemyVisualProfile visualProfile;
        private FormalBattleAnimationClip currentClip;
        private C1EnemyPresentationVisualState currentState;
        private string actorBalanceId = string.Empty;
        private string contentId = string.Empty;
        private string runtimeProfileId = string.Empty;
        private int maxHp;
        private int currentHp;
        private float clipStartedAt;
        private float hitStartedAt = -100f;
        private float telegraphStartedAt = -100f;
        private bool defeated;
        private bool paused;
        private float pausedAt;
        private Vector2 visualBaselinePosition;

        public int AuthoredStableOrder => authoredStableOrder;
        public string ActorBalanceId => actorBalanceId;
        public bool Defeated => defeated;
        public bool Bound => !string.IsNullOrEmpty(actorBalanceId);
        public RectTransform DamageAnchor =>
            feedbackReceiverLanes != null
                ? feedbackReceiverLanes.IncomingDamageLane
                : damageAnchor != null ? damageAnchor : visualRect;
        public RectTransform EffectFloatAnchor =>
            feedbackReceiverLanes != null
                ? feedbackReceiverLanes.BenefitLane
                : effectFloatAnchor;
        public RectTransform StatusFloatAnchor =>
            feedbackReceiverLanes == null
                ? null
                : feedbackReceiverLanes.StatusLane;
        public FormalBattleFeedbackReceiverLanes FeedbackReceiverLanes =>
            feedbackReceiverLanes;
#if UNITY_EDITOR
        public int EditorTelegraphPlayCount { get; private set; }
        public int EditorAttackPlayCount { get; private set; }
#endif

        private void Update()
        {
            if (paused || !Bound || currentClip == null || defeated
                && currentState != C1EnemyPresentationVisualState.Death)
            {
                return;
            }

            float now = Time.unscaledTime;
            float elapsed = Mathf.Max(0f, now - clipStartedAt);
            Texture2D[] frames = currentClip.Frames;
            if (frames.Length > 0)
            {
                int frameIndex = Mathf.FloorToInt(
                    elapsed * 1000f / currentClip.FrameDurationMilliseconds);
                frameIndex = currentClip.Loops
                    ? frameIndex % frames.Length
                    : Mathf.Clamp(frameIndex, 0, frames.Length - 1);
                ApplyFrame(frames[frameIndex]);
            }

            if (!currentClip.Loops
                && elapsed >= currentClip.DurationSeconds)
            {
                if (currentState == C1EnemyPresentationVisualState.Death)
                {
                    Color color = visualImage.color;
                    color.a = 0.36f;
                    visualImage.color = color;
                }
                else
                {
                    SetState(C1EnemyPresentationVisualState.Idle);
                }
            }

            AnimateHit(now);
            AnimateTelegraph(now);
        }

        public bool Bind(
            C1FormalRealtimeBattleActorSnapshot snapshot,
            FormalBattlePresentationProfile profile,
            out string diagnostic)
        {
            diagnostic = string.Empty;
            if (snapshot == null
                || snapshot.stableActorOrder != authoredStableOrder
                || profile == null
                || !profile.TryGetEnemyProfile(
                    snapshot.contentId,
                    snapshot.runtimeProfileId,
                    out FormalBattleEnemyVisualProfile resolved))
            {
                diagnostic = "FORMAL_ENEMY_SLOT_IDENTITY_REJECTED_"
                    + authoredStableOrder;
                return false;
            }

            bool identityChanged = !string.Equals(
                    actorBalanceId,
                    snapshot.actorBalanceId,
                    StringComparison.Ordinal)
                || !string.Equals(contentId, snapshot.contentId,
                    StringComparison.Ordinal)
                || !string.Equals(runtimeProfileId, snapshot.runtimeProfileId,
                    StringComparison.Ordinal);
            actorBalanceId = snapshot.actorBalanceId;
            contentId = snapshot.contentId;
            runtimeProfileId = snapshot.runtimeProfileId;
            visualProfile = resolved;
            maxHp = Mathf.Max(1, snapshot.maxHp);
            currentHp = Mathf.Clamp(snapshot.currentHp, 0, maxHp);
            defeated = snapshot.defeated;

            if (identityChanged)
            {
                if (visualRect != null)
                {
                    visualBaselinePosition = visualRect.anchoredPosition;
                }
                hitStartedAt = -100f;
                telegraphStartedAt = -100f;
                SetState(defeated
                    ? C1EnemyPresentationVisualState.Death
                    : C1EnemyPresentationVisualState.Idle);
            }
            else if (defeated
                     && currentState != C1EnemyPresentationVisualState.Death)
            {
                SetState(C1EnemyPresentationVisualState.Death);
            }

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            diagnostic = "FORMAL_ENEMY_SLOT_BOUND_" + actorBalanceId;
            return true;
        }

        public void ApplyHp(int value, int maximum)
        {
            maxHp = Mathf.Max(1, maximum);
            currentHp = Mathf.Clamp(value, 0, maxHp);
            defeated = currentHp <= 0;
        }

        public bool ApplyObservability(
            C1FormalRealtimeBattleActorSnapshot snapshot,
            IReadOnlyList<C1FormalRealtimeBattleStatusSnapshot> statuses,
            FormalBattlePresentationProfile profile,
            long battleTimeMs,
            out string diagnostic)
        {
            if (snapshot == null
                || !Bound
                || !string.Equals(
                    actorBalanceId,
                    snapshot.actorBalanceId,
                    StringComparison.Ordinal)
                || snapshot.stableActorOrder != authoredStableOrder)
            {
                diagnostic = "FORMAL_ENEMY_OBSERVABILITY_IDENTITY_REJECTED_"
                    + authoredStableOrder;
                return false;
            }

            ApplyHp(snapshot.currentHp, snapshot.maxHp);
            diagnostic = "FORMAL_ENEMY_ACTOR_STAGE_BOUND_"
                + actorBalanceId;
            return true;
        }

        public void SetTarget(bool selected)
        {
            if (targetOutline != null)
            {
                targetOutline.enabled = selected && Bound && !defeated;
            }
        }

        public void PlayTelegraph()
        {
            if (!defeated)
            {
#if UNITY_EDITOR
                EditorTelegraphPlayCount++;
#endif
                telegraphStartedAt = Time.unscaledTime;
            }
        }

        public void PlayAttack()
        {
            if (!defeated)
            {
#if UNITY_EDITOR
                EditorAttackPlayCount++;
#endif
                SetState(C1EnemyPresentationVisualState.BasicAttack);
            }
        }

        public void PlayHit()
        {
            if (defeated)
            {
                return;
            }
            hitStartedAt = Time.unscaledTime;
            if (currentState != C1EnemyPresentationVisualState.BasicAttack)
            {
                SetState(C1EnemyPresentationVisualState.Hit);
            }
        }

        public void PlayDeath()
        {
            if (defeated
                && currentState == C1EnemyPresentationVisualState.Death)
            {
                return;
            }
            defeated = true;
            SetTarget(false);
            SetState(C1EnemyPresentationVisualState.Death);
        }

        public void SetPaused(bool value)
        {
            if (paused == value)
            {
                return;
            }
            paused = value;
            if (paused)
            {
                pausedAt = Time.unscaledTime;
            }
            else
            {
                float shift = Mathf.Max(0f, Time.unscaledTime - pausedAt);
                clipStartedAt += shift;
                hitStartedAt += shift;
                telegraphStartedAt += shift;
            }
        }

        public void Clear()
        {
            actorBalanceId = string.Empty;
            contentId = string.Empty;
            runtimeProfileId = string.Empty;
            visualProfile = null;
            currentClip = null;
            defeated = false;
            paused = false;
            pausedAt = 0f;
#if UNITY_EDITOR
            EditorTelegraphPlayCount = 0;
            EditorAttackPlayCount = 0;
#endif
            SetTarget(false);
            if (visualImage != null)
            {
                visualImage.sprite = null;
                visualImage.enabled = false;
                visualImage.color = Color.white;
            }
            if (visualRect != null)
            {
                visualRect.anchoredPosition = visualBaselinePosition;
            }
            if (hitOverlayImage != null)
            {
                Color color = hitOverlayImage.color;
                color.a = 0f;
                hitOverlayImage.color = color;
            }
            if (telegraphGroup != null) telegraphGroup.alpha = 0f;
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        public bool ValidateAuthoredReferences()
        {
            return authoredStableOrder >= 0
                   && authoredStableOrder <= 2
                   && visualImage != null
                   && visualRect != null
                   && targetOutline != null
                   && calibrationEffect != null
                   && telegraphGroup != null
                   && hitOverlayImage != null
                   && DamageAnchor != null
                   && EffectFloatAnchor != null
                   && StatusFloatAnchor != null
                   && feedbackReceiverLanes != null
                   && feedbackReceiverLanes.ValidateAuthoredReferences()
                   && !visualImage.raycastTarget
                   && !hitOverlayImage.raycastTarget;
        }

        private void SetState(C1EnemyPresentationVisualState state)
        {
            if (visualProfile == null)
            {
                return;
            }
            FormalBattleAnimationClip clip = state switch
            {
                C1EnemyPresentationVisualState.BasicAttack =>
                    visualProfile.Attack,
                C1EnemyPresentationVisualState.Hit => visualProfile.Hit,
                C1EnemyPresentationVisualState.Death => visualProfile.Death,
                _ => visualProfile.Idle
            };
            if (clip == null || clip.Frames.Length == 0)
            {
                return;
            }
            currentState = state;
            currentClip = clip;
            clipStartedAt = Time.unscaledTime;
            if (visualImage != null)
            {
                visualImage.enabled = true;
                visualImage.color = Color.white;
            }
            if (calibrationEffect != null)
            {
                calibrationEffect.Apply(new C1EnemyVisualCalibration(
                    clip.ReferencePixelBounds,
                    clip.UnionPixelBounds,
                    1f,
                    Vector2.zero,
                    new Vector2(0.5f, 0.5f)));
            }
            ApplyFrame(clip.Frames[0]);
        }

        private void ApplyFrame(Texture2D texture)
        {
            if (texture == null || visualImage == null)
            {
                return;
            }
            if (!runtimeSprites.TryGetValue(texture, out Sprite sprite)
                || sprite == null)
            {
                sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect);
                sprite.name = "FormalBattle_" + texture.name;
                sprite.hideFlags = HideFlags.DontSave;
                runtimeSprites[texture] = sprite;
            }
            visualImage.sprite = sprite;
        }

        private void AnimateHit(float now)
        {
            float progress = Mathf.Clamp01((now - hitStartedAt) / 0.24f);
            if (hitOverlayImage != null)
            {
                hitOverlayImage.sprite = visualImage == null
                    ? null
                    : visualImage.sprite;
                Color color = hitOverlayImage.color;
                color.a = progress < 1f
                    ? Mathf.Sin(progress * Mathf.PI) * 0.68f
                    : 0f;
                hitOverlayImage.color = color;
            }
            if (visualRect != null)
            {
                float recoil = progress < 1f
                    ? Mathf.Sin(progress * Mathf.PI * 2f) * 7f
                    : 0f;
                visualRect.anchoredPosition = visualBaselinePosition
                    + Vector2.right * recoil;
            }
        }

        private void AnimateTelegraph(float now)
        {
            if (telegraphGroup == null)
            {
                return;
            }
            float progress = Mathf.Clamp01(
                (now - telegraphStartedAt) / 0.7f);
            telegraphGroup.alpha = progress < 1f
                ? Mathf.Sin(progress * Mathf.PI) * 0.9f
                : 0f;
        }

        private void OnDestroy()
        {
            foreach (Sprite sprite in runtimeSprites.Values)
            {
                if (sprite == null) continue;
                if (Application.isPlaying) Destroy(sprite);
                else DestroyImmediate(sprite);
            }
            runtimeSprites.Clear();
        }

#if UNITY_EDITOR
        public void AssignStableOrderForEditor(int configuredStableOrder)
        {
            authoredStableOrder = configuredStableOrder;
        }

        public void AssignForEditor(
            int configuredStableOrder,
            Image configuredVisualImage,
            RectTransform configuredVisualRect,
            C1AuthoredEnemySelectionOutlineEffect configuredTargetOutline,
            C1AuthoredEnemyCalibrationMeshEffect configuredCalibrationEffect,
            Image configuredHpFillImage,
            TMP_Text configuredHpLabel,
            TMP_Text configuredIdentityLabel,
            CanvasGroup configuredTelegraphGroup,
            Image configuredHitOverlayImage,
            RectTransform configuredDamageAnchor)
        {
            authoredStableOrder = configuredStableOrder;
            visualImage = configuredVisualImage;
            visualRect = configuredVisualRect;
            targetOutline = configuredTargetOutline;
            calibrationEffect = configuredCalibrationEffect;
            telegraphGroup = configuredTelegraphGroup;
            hitOverlayImage = configuredHitOverlayImage;
            damageAnchor = configuredDamageAnchor;
        }

        public void AssignReadabilityForEditor(
            GameObject configuredShellRoot,
            Image configuredShellFillImage,
            TMP_Text configuredShellLabel,
            FormalBattleStatusStripView configuredStatusStrip)
        {
            // Retained only so older editor authoring code compiles. Actor
            // stages no longer own HUD/readability references.
        }

        public void AssignEffectFloatAnchorForEditor(
            RectTransform configuredEffectFloatAnchor)
        {
            effectFloatAnchor = configuredEffectFloatAnchor;
        }

        public void AssignFeedbackReceiverLanesForEditor(
            FormalBattleFeedbackReceiverLanes configuredLanes)
        {
            feedbackReceiverLanes = configuredLanes;
        }
#endif
    }
}
