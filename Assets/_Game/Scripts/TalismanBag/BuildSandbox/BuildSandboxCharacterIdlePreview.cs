using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TalismanBag.BuildSandbox
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class BuildSandboxCharacterIdlePreview : MonoBehaviour
    {
        public const string PackageName = "V0.4-CharacterIdlePreview01";
        public const string DefaultResourcesFramesPath =
            "anim/video_6a56065e5485ff99517e29da_1784022639683";

        [SerializeField] private bool devOnly = true;
        [SerializeField] private bool isEnabled = true;
        [SerializeField] private Image targetImage;
        [SerializeField] private bool preferInspectorFrameSlots = true;
        [SerializeField] private List<Sprite> inspectorFrameSlots = new();
        [SerializeField] private string resourcesFramesPath = DefaultResourcesFramesPath;
        [SerializeField, Min(1f)] private float framesPerSecond = 8f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool preserveAspect = true;
        [SerializeField] private bool showFirstFrameWhenStopped = true;
        [SerializeField] private bool loadTexturesWhenSpritesMissing = true;

        private readonly List<Sprite> generatedSprites = new();
        private List<Sprite> cachedResourceFrames;
        private string cachedResourceFramesPath = string.Empty;
        private bool isPlaying;
        private int frameIndex;
        private float frameTimer;

        public bool DevOnly => devOnly;
        public bool IsEnabled => isEnabled;
        public int FrameCount => ResolveFrames().Count;

        public void PlayFromStart()
        {
            EnsureDefaults();
            List<Sprite> frames = ResolveFrames();
            if (!isEnabled || targetImage == null || frames.Count == 0)
            {
                return;
            }

            isPlaying = true;
            frameIndex = 0;
            frameTimer = 0f;
            ApplyFrame(frames, frameIndex);
        }

        public void Stop()
        {
            isPlaying = false;
            frameTimer = 0f;
            if (showFirstFrameWhenStopped)
            {
                ApplyFirstFrame();
            }
        }

        public void EnsureDefaults()
        {
            EnsureTargetImage();
            if (targetImage == null)
            {
                return;
            }

            targetImage.raycastTarget = false;
            targetImage.preserveAspect = preserveAspect;
            if (targetImage.color.a <= 0f)
            {
                targetImage.color = Color.white;
            }

            if (!Application.isPlaying)
            {
                ApplyFirstFrame();
            }
        }

        private void Awake()
        {
            EnsureDefaults();
        }

        private void OnEnable()
        {
            EnsureDefaults();
            if (playOnEnable)
            {
                PlayFromStart();
            }
        }

        private void OnValidate()
        {
            framesPerSecond = Mathf.Max(1f, framesPerSecond);
            EnsureDefaults();
        }

        private void OnDestroy()
        {
            DestroyGeneratedSprites();
        }

        private void Update()
        {
            if (!isEnabled || !isPlaying || targetImage == null)
            {
                return;
            }

            List<Sprite> frames = ResolveFrames();
            if (frames.Count == 0)
            {
                return;
            }

            float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
            frameTimer += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            while (frameTimer >= frameDuration && isPlaying)
            {
                frameTimer -= frameDuration;
                AdvanceFrame(frames);
            }
        }

        private void AdvanceFrame(IReadOnlyList<Sprite> frames)
        {
            frameIndex++;
            if (frameIndex < frames.Count)
            {
                ApplyFrame(frames, frameIndex);
                return;
            }

            if (loop)
            {
                frameIndex = 0;
                ApplyFrame(frames, frameIndex);
                return;
            }

            isPlaying = false;
            frameIndex = Mathf.Max(0, frames.Count - 1);
            if (showFirstFrameWhenStopped)
            {
                ApplyFrame(frames, 0);
            }
        }

        private void ApplyFirstFrame()
        {
            List<Sprite> frames = ResolveFrames();
            if (frames.Count > 0)
            {
                ApplyFrame(frames, 0);
            }
        }

        private void ApplyFrame(IReadOnlyList<Sprite> frames, int index)
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

        private List<Sprite> ResolveFrames()
        {
            if (TryResolveInspectorFrames(out List<Sprite> inspectorFrames))
            {
                return inspectorFrames;
            }

            string path = string.IsNullOrWhiteSpace(resourcesFramesPath)
                ? DefaultResourcesFramesPath
                : resourcesFramesPath.Trim();
            if (cachedResourceFrames != null
                && string.Equals(cachedResourceFramesPath, path, StringComparison.Ordinal))
            {
                return cachedResourceFrames;
            }

            cachedResourceFramesPath = path;
            cachedResourceFrames = LoadResourceFrames(path);
            return cachedResourceFrames;
        }

        private bool TryResolveInspectorFrames(out List<Sprite> frames)
        {
            frames = new List<Sprite>();
            if (!preferInspectorFrameSlots || inspectorFrameSlots == null)
            {
                return false;
            }

            for (int i = 0; i < inspectorFrameSlots.Count; i++)
            {
                Sprite frame = inspectorFrameSlots[i];
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }

            return frames.Count > 0;
        }

        private List<Sprite> LoadResourceFrames(string path)
        {
            List<Sprite> frames = new(Resources.LoadAll<Sprite>(path) ?? Array.Empty<Sprite>());
            frames.RemoveAll(frame => frame == null);
            if (frames.Count == 0 && loadTexturesWhenSpritesMissing)
            {
                foreach (Texture2D texture in Resources.LoadAll<Texture2D>(path) ?? Array.Empty<Texture2D>())
                {
                    if (texture == null)
                    {
                        continue;
                    }

                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(0f, 0f, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f);
                    sprite.name = texture.name;
                    sprite.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                    generatedSprites.Add(sprite);
                    frames.Add(sprite);
                }
            }

            frames.Sort((left, right) => StringComparer.OrdinalIgnoreCase.Compare(left.name, right.name));
            return frames;
        }

        private void EnsureTargetImage()
        {
            if (targetImage == null)
            {
                targetImage = GetComponent<Image>();
            }
        }

        private void DestroyGeneratedSprites()
        {
            for (int i = generatedSprites.Count - 1; i >= 0; i--)
            {
                UnityEngine.Object obj = generatedSprites[i];
                if (obj == null)
                {
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(obj);
                }
                else
                {
                    DestroyImmediate(obj);
                }
            }

            generatedSprites.Clear();
            cachedResourceFrames = null;
            cachedResourceFramesPath = string.Empty;
        }
    }
}
