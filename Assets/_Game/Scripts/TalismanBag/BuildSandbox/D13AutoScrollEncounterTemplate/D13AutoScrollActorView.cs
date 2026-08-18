using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox.D13AutoScrollEncounterTemplate
{
    internal sealed class D13AutoScrollActorView : MonoBehaviour
    {
        private readonly Dictionary<D13AutoScrollAnimationSlot, Sprite[]>
            framesBySlot = new();

        private readonly Dictionary<
            D13AutoScrollAnimationSlot,
            D13AutoScrollSequenceProfile> profilesBySlot = new();

        private readonly HashSet<D13AutoScrollAnimationSlot> warnedSlots = new();
        private readonly List<Sprite> ownedSprites = new();

        private RectTransform runCarrier;
        private RectTransform visualRect;
        private Image visualImage;
        private D13AutoScrollEncounterPresentationProfile encounterProfile;
        private D13AutoScrollAnimationSlot currentSlot;
        private D13AutoScrollAnimationSlot resumeSlot;
        private float playbackElapsed;
        private float runPhase;
        private bool configured;
        private bool runSurrogateEnabled;
        private bool oneShotPlaying;
        private bool deathLocked;
        private bool staticSource;
        private Vector2 staticVisualPosition;
        private Vector3 staticVisualScale = Vector3.one;
        private Quaternion staticVisualRotation = Quaternion.identity;
        private Color staticVisualColor = Color.white;

        public bool IsOneShotPlaying => oneShotPlaying;

        public bool IsDeathLocked => deathLocked;

        public D13AutoScrollAnimationSlot CurrentSlot => currentSlot;

        public int LiveOwnedSpriteCount => ownedSprites.Count;

        public float CurrentFootLineOffset =>
            visualRect == null ? 0f : visualRect.anchoredPosition.y;

        public void ConfigureD13(
            RectTransform targetRunCarrier,
            RectTransform targetVisualRect,
            Image targetImage,
            D13AutoScrollEncounterPresentationProfile profile)
        {
            runCarrier = targetRunCarrier;
            visualRect = targetVisualRect;
            visualImage = targetImage;
            encounterProfile = profile;
            staticSource = false;
            configured = runCarrier != null
                && visualRect != null
                && visualImage != null
                && encounterProfile != null;

            if (!configured)
            {
                return;
            }

            DestroyOwnedSprites();
            framesBySlot.Clear();
            profilesBySlot.Clear();
            warnedSlots.Clear();

            D13AutoScrollSequenceProfile[] sequences =
                encounterProfile.CreateEnemySequences();
            for (int i = 0; i < sequences.Length; i++)
            {
                D13AutoScrollSequenceProfile sequence = sequences[i];
                profilesBySlot[sequence.Slot] = sequence;
                framesBySlot[sequence.Slot] = LoadFrames(sequence);
            }

            ResetRound();
        }

        public void ConfigureStatic(
            RectTransform targetRunCarrier,
            RectTransform targetVisualRect,
            Image targetImage,
            Sprite sourceSprite,
            Vector2 sourceSize,
            Vector2 sourcePivot,
            Color sourceColor)
        {
            runCarrier = targetRunCarrier;
            visualRect = targetVisualRect;
            visualImage = targetImage;
            encounterProfile =
                D13AutoScrollEncounterPresentationProfile.Default;
            staticSource = true;
            configured = runCarrier != null
                && visualRect != null
                && visualImage != null
                && sourceSprite != null;

            if (!configured)
            {
                return;
            }

            DestroyOwnedSprites();
            framesBySlot.Clear();
            profilesBySlot.Clear();
            warnedSlots.Clear();

            visualImage.sprite = sourceSprite;
            visualImage.color = sourceColor;
            visualImage.preserveAspect = true;
            visualImage.raycastTarget = false;
            visualRect.anchorMin = new Vector2(0.5f, 0.5f);
            visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.pivot = sourcePivot;
            visualRect.sizeDelta = sourceSize;
            visualRect.anchoredPosition = Vector2.zero;
            staticVisualPosition = visualRect.anchoredPosition;
            staticVisualScale = visualRect.localScale;
            staticVisualRotation = visualRect.localRotation;
            staticVisualColor = sourceColor;
            ResetRound();
        }

        public void ResetRound()
        {
            if (!configured)
            {
                return;
            }

            deathLocked = false;
            oneShotPlaying = false;
            runSurrogateEnabled = false;
            playbackElapsed = 0f;
            runPhase = 0f;
            runCarrier.anchoredPosition = Vector2.zero;
            runCarrier.localRotation = Quaternion.identity;
            runCarrier.localScale = Vector3.one;

            if (staticSource)
            {
                visualRect.anchoredPosition = staticVisualPosition;
                visualRect.localScale = staticVisualScale;
                visualRect.localRotation = staticVisualRotation;
                visualImage.color = staticVisualColor;
                return;
            }

            ApplySlot(D13AutoScrollAnimationSlot.Idle, true);
        }

        public void SetRunSurrogate(bool enabled, float phaseOffset = 0f)
        {
            if (!configured || deathLocked)
            {
                return;
            }

            runSurrogateEnabled = enabled;
            runPhase = phaseOffset;
            if (!enabled)
            {
                runCarrier.anchoredPosition = Vector2.zero;
                runCarrier.localRotation = Quaternion.identity;
                runCarrier.localScale = Vector3.one;
            }
        }

        public bool PlayOneShot(
            D13AutoScrollAnimationSlot slot,
            D13AutoScrollAnimationSlot slotAfterCompletion)
        {
            if (!configured || deathLocked || staticSource)
            {
                return false;
            }

            resumeSlot = slotAfterCompletion;
            oneShotPlaying = true;
            runSurrogateEnabled = false;
            ApplySlot(slot, true);
            return true;
        }

        public bool PlayHeavyHit()
        {
            return PlayOneShot(
                D13AutoScrollAnimationSlot.HeavyHit,
                D13AutoScrollAnimationSlot.Idle);
        }

        public void InterruptHeavyHitAndRestore()
        {
            if (!configured
                || deathLocked
                || currentSlot != D13AutoScrollAnimationSlot.HeavyHit)
            {
                return;
            }

            oneShotPlaying = false;
            ApplySlot(D13AutoScrollAnimationSlot.Idle, true);
        }

        public void LockDeath()
        {
            if (!configured || staticSource)
            {
                return;
            }

            deathLocked = true;
            oneShotPlaying = true;
            runSurrogateEnabled = false;
            ApplySlot(D13AutoScrollAnimationSlot.Death, true);
        }

        private void Update()
        {
            if (!configured)
            {
                return;
            }

            float deltaTime = Mathf.Max(0f, Time.deltaTime);
            UpdateRunSurrogate(deltaTime);
            if (staticSource)
            {
                return;
            }

            Sprite[] frames = ResolveFrames(currentSlot);
            D13AutoScrollSequenceProfile profile = ResolveProfile(currentSlot);
            if (frames.Length == 0 || profile == null)
            {
                return;
            }

            playbackElapsed += deltaTime;
            float duration = frames.Length * profile.FrameSeconds;
            if (oneShotPlaying && playbackElapsed >= duration)
            {
                visualImage.sprite = frames[frames.Length - 1];
                if (deathLocked)
                {
                    oneShotPlaying = false;
                    return;
                }

                oneShotPlaying = false;
                ApplySlot(resumeSlot, true);
                return;
            }

            int frameIndex = Mathf.FloorToInt(
                playbackElapsed / profile.FrameSeconds);
            if (oneShotPlaying)
            {
                frameIndex = Mathf.Clamp(frameIndex, 0, frames.Length - 1);
            }
            else
            {
                frameIndex %= frames.Length;
            }

            visualImage.sprite = frames[frameIndex];
        }

        private void UpdateRunSurrogate(float deltaTime)
        {
            if (!runSurrogateEnabled)
            {
                return;
            }

            runPhase += deltaTime * 11f;
            float bob = Mathf.Sin(runPhase)
                * encounterProfile.RunBobAmplitude;
            float pulse = 1f + (Mathf.Sin(runPhase * 2f) * 0.015f);
            runCarrier.anchoredPosition = new Vector2(0f, bob);
            runCarrier.localRotation = Quaternion.Euler(
                0f,
                0f,
                -encounterProfile.RunLeanDegrees);
            runCarrier.localScale = new Vector3(pulse, 2f - pulse, 1f);
        }

        private void ApplySlot(
            D13AutoScrollAnimationSlot slot,
            bool restart)
        {
            currentSlot = slot;
            if (restart)
            {
                playbackElapsed = 0f;
            }

            D13AutoScrollSequenceProfile profile = ResolveProfile(slot);
            Sprite[] frames = ResolveFrames(slot);
            if (profile == null || frames.Length == 0)
            {
                visualImage.sprite = null;
                return;
            }

            visualRect.anchorMin = new Vector2(0.5f, 0.5f);
            visualRect.anchorMax = new Vector2(0.5f, 0.5f);
            visualRect.pivot = new Vector2(0.5f, 0f);
            visualRect.sizeDelta = profile.VisualSize;
            visualRect.anchoredPosition = profile.FootLineOffset;
            visualRect.localScale = Vector3.one;
            visualRect.localRotation = Quaternion.identity;
            visualImage.sprite = frames[0];
            visualImage.preserveAspect = true;
            visualImage.raycastTarget = false;
            visualImage.color = Color.white;
        }

        private Sprite[] LoadFrames(D13AutoScrollSequenceProfile profile)
        {
            Texture2D[] textures =
                Resources.LoadAll<Texture2D>(profile.ResourcePath)
                ?? Array.Empty<Texture2D>();
            Array.Sort(
                textures,
                (left, right) => StringComparer.OrdinalIgnoreCase.Compare(
                    left == null ? string.Empty : left.name,
                    right == null ? string.Empty : right.name));

            List<Sprite> frames = new(textures.Length);
            for (int i = 0; i < textures.Length; i++)
            {
                Texture2D texture = textures[i];
                if (texture == null)
                {
                    continue;
                }

                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                if (sprite == null)
                {
                    continue;
                }

                sprite.name = "D13Owned_" + texture.name;
                sprite.hideFlags =
                    HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                ownedSprites.Add(sprite);
                frames.Add(sprite);
            }

            return frames.ToArray();
        }

        private Sprite[] ResolveFrames(D13AutoScrollAnimationSlot slot)
        {
            if (framesBySlot.TryGetValue(slot, out Sprite[] frames)
                && frames != null
                && frames.Length > 0)
            {
                return frames;
            }

            WarnMissingSlotOnce(slot);
            if (framesBySlot.TryGetValue(
                    D13AutoScrollAnimationSlot.Idle,
                    out Sprite[] idleFrames)
                && idleFrames != null
                && idleFrames.Length > 0)
            {
                return idleFrames;
            }

            foreach (Sprite[] candidate in framesBySlot.Values)
            {
                if (candidate != null && candidate.Length > 0)
                {
                    return candidate;
                }
            }

            return Array.Empty<Sprite>();
        }

        private D13AutoScrollSequenceProfile ResolveProfile(
            D13AutoScrollAnimationSlot slot)
        {
            if (profilesBySlot.TryGetValue(
                    slot,
                    out D13AutoScrollSequenceProfile profile))
            {
                return profile;
            }

            WarnMissingSlotOnce(slot);
            profilesBySlot.TryGetValue(
                D13AutoScrollAnimationSlot.Idle,
                out D13AutoScrollSequenceProfile idleProfile);
            return idleProfile;
        }

        private void WarnMissingSlotOnce(D13AutoScrollAnimationSlot slot)
        {
            if (!warnedSlots.Add(slot))
            {
                return;
            }

            Debug.LogWarning(
                "[D13AutoScrollEncounter] Missing d1_3 "
                + slot
                + " sequence; using Idle/static fallback.");
        }

        private void OnDestroy()
        {
            DestroyOwnedSprites();
        }

        private void DestroyOwnedSprites()
        {
            for (int i = 0; i < ownedSprites.Count; i++)
            {
                Sprite sprite = ownedSprites[i];
                if (sprite != null)
                {
                    Destroy(sprite);
                }
            }

            ownedSprites.Clear();
        }
    }
}
