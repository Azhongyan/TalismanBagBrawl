using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    [DisallowMultipleComponent]
    public sealed class BuildSandboxSpriteSequencePlayer : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private List<Sprite> frames = new();
        [SerializeField, Min(1f)] private float framesPerSecond = 18f;
        [SerializeField] private bool loop;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool restoreFirstFrameWhenStopped = true;

        private Sprite originalSprite;
        private bool hasOriginalSprite;
        private bool isPlaying;
        private int frameIndex;
        private float frameTimer;

        public bool IsPlaying => isPlaying;
        public int FrameCount => frames?.Count ?? 0;

        public void PlayFromStart()
        {
            EnsureTargetImage();
            if (targetImage == null || frames == null || frames.Count == 0)
            {
                return;
            }

            if (!hasOriginalSprite)
            {
                originalSprite = targetImage.sprite;
                hasOriginalSprite = true;
            }

            isPlaying = true;
            frameIndex = 0;
            frameTimer = 0f;
            ApplyFrame(frameIndex);
        }

        public void Stop()
        {
            isPlaying = false;
            frameTimer = 0f;
            if (restoreFirstFrameWhenStopped && frames != null && frames.Count > 0)
            {
                ApplyFrame(0);
            }
            else if (targetImage != null && hasOriginalSprite)
            {
                targetImage.sprite = originalSprite;
            }
        }

        private void Awake()
        {
            EnsureTargetImage();
        }

        private void OnValidate()
        {
            EnsureTargetImage();
            framesPerSecond = Mathf.Max(1f, framesPerSecond);
        }

        private void Update()
        {
            if (!isPlaying || targetImage == null || frames == null || frames.Count == 0)
            {
                return;
            }

            float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
            frameTimer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            while (frameTimer >= frameDuration && isPlaying)
            {
                frameTimer -= frameDuration;
                AdvanceFrame();
            }
        }

        private void AdvanceFrame()
        {
            frameIndex++;
            if (frameIndex < frames.Count)
            {
                ApplyFrame(frameIndex);
                return;
            }

            if (loop)
            {
                frameIndex = 0;
                ApplyFrame(frameIndex);
                return;
            }

            isPlaying = false;
            frameIndex = Mathf.Max(0, frames.Count - 1);
            if (restoreFirstFrameWhenStopped)
            {
                ApplyFrame(0);
            }
        }

        private void ApplyFrame(int index)
        {
            if (targetImage == null || frames == null || index < 0 || index >= frames.Count)
            {
                return;
            }

            Sprite sprite = frames[index];
            if (sprite != null)
            {
                targetImage.sprite = sprite;
            }
        }

        private void EnsureTargetImage()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }
        }
    }
}
