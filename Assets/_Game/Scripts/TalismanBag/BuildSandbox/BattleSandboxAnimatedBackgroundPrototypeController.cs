using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.Collections;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace TalismanBag.BuildSandbox
{
    /// <summary>
    /// Dev-only, one-off animated-background presentation prototype.
    /// It binds the authored gameBackground Image only in the V0.4 BattleSandbox scene,
    /// never writes layout, and owns no battle/runtime truth.
    ///
    /// Prototype memory note: eleven 1672x941 RGBA32 frames represent an upper-bound
    /// decoded footprint of about 66 MiB on one memory side, before any platform-specific
    /// duplication. This intentionally simple full-resolution test is not an atlas,
    /// streaming, compression, or production background solution.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BattleSandboxAnimatedBackgroundPrototypeController : MonoBehaviour
    {
        public const string PackageId =
            "V0.4-BattleSandboxAnimatedBackgroundPrototype01";
        public const string TargetSceneName =
            "Scene_TalismanBag_V04_BattleSandboxPreview";
        public const string TargetObjectName = "gameBackground";

        private const string ResourcePath = "场景地图/场景动画序列帧";
        private const int FrameCount = 11;
        private const float FrameDurationSeconds = 0.10f;
        private const float HoldDurationSeconds = 5.0f;

        private enum PlaybackPhase
        {
            Frames,
            HoldFrameOne
        }

        private struct RectTransformSnapshot
        {
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
            public Vector2 Pivot;
            public Vector3 LocalPosition;
            public Vector3 LocalScale;
            public Quaternion LocalRotation;
            public Transform Parent;
            public int SiblingIndex;
        }

        private static bool bootstrapRegistered;
        private static int warningSceneHandle = int.MinValue;

        private readonly List<Sprite> runtimeSprites =
            new List<Sprite>(FrameCount);

        private Image targetImage;
        private RectTransform targetRect;
        private Sprite originalSprite;
        private Color originalColor;
        private Material originalMaterial;
        private bool originalUsedDefaultMaterial;
        private bool originalRaycastTarget;
        private bool originalPreserveAspect;
        private bool originalImageEnabled;
        private RectTransformSnapshot originalRect;
        private bool originalCached;
        private bool initialized;
        private bool instanceWarningReported;
        private PlaybackPhase playbackPhase;
        private int currentFrameIndex = -1;
        private float phaseElapsed;
        private double holdStartedAt;
        private int completedSequenceCount;
        private int completedHoldCount;
        private int frameApplyCount;
        private int runtimeSpriteCreationCount;
        private int playbackOrderFaultCount;
        private int expectedFrameIndex;
        private float lastCompletedHoldSeconds;
        private float minimumCompletedHoldSeconds = float.PositiveInfinity;

        public bool DevOnly
        {
            get { return true; }
        }

        public bool OwnsBattleOrEnemyTruth
        {
            get { return false; }
        }

        public bool WritesFormalFlow
        {
            get { return false; }
        }

        public bool IsInitialized
        {
            get { return initialized; }
        }

        public bool IsHoldingFrameOne
        {
            get
            {
                return initialized &&
                    playbackPhase == PlaybackPhase.HoldFrameOne;
            }
        }

        public int CurrentFrameIndex
        {
            get { return currentFrameIndex; }
        }

        public int CompletedSequenceCount
        {
            get { return completedSequenceCount; }
        }

        public int CompletedHoldCount
        {
            get { return completedHoldCount; }
        }

        public float LastCompletedHoldSeconds
        {
            get { return lastCompletedHoldSeconds; }
        }

        public float MinimumCompletedHoldSeconds
        {
            get
            {
                return float.IsPositiveInfinity(minimumCompletedHoldSeconds)
                    ? 0f
                    : minimumCompletedHoldSeconds;
            }
        }

        public int RuntimeSpriteCreationCount
        {
            get { return runtimeSpriteCreationCount; }
        }

        public int LiveRuntimeSpriteCount
        {
            get { return runtimeSprites.Count; }
        }

        public int PlaybackOrderFaultCount
        {
            get { return playbackOrderFaultCount; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterBootstrap()
        {
            if (bootstrapRegistered)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            bootstrapRegistered = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AttachToCurrentScene()
        {
            TryAttach(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            warningSceneHandle = int.MinValue;
            TryAttach(scene);
        }

        private static void TryAttach(Scene scene)
        {
            if (!scene.IsValid() ||
                !scene.isLoaded ||
                !string.Equals(
                    scene.name,
                    TargetSceneName,
                    StringComparison.Ordinal))
            {
                return;
            }

            Image image;
            string failure;
            if (!TryFindExactTarget(scene, out image, out failure))
            {
                WarnSceneOnce(scene, failure);
                return;
            }

            if (image.GetComponent<
                    BattleSandboxAnimatedBackgroundPrototypeController>() ==
                null)
            {
                image.gameObject.AddComponent<
                    BattleSandboxAnimatedBackgroundPrototypeController>();
            }
        }

        private static bool TryFindExactTarget(
            Scene scene,
            out Image image,
            out string failure)
        {
            image = null;
            failure = string.Empty;
            int objectMatches = 0;
            Transform[] transforms =
                UnityEngine.Object.FindObjectsOfType<Transform>(true);

            for (int i = 0; i < transforms.Length; i++)
            {
                Transform candidate = transforms[i];
                if (candidate == null ||
                    candidate.gameObject.scene != scene ||
                    !string.Equals(
                        candidate.name,
                        TargetObjectName,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                objectMatches++;
                Image[] images = candidate.GetComponents<Image>();
                if (images.Length == 1)
                {
                    image = images[0];
                }
                else
                {
                    failure =
                        "Exact gameBackground must contain one existing UI Image; authored background was left unchanged.";
                    image = null;
                }
            }

            if (objectMatches != 1)
            {
                failure =
                    "Expected one exact gameBackground object in the authorized scene; authored background was left unchanged.";
                image = null;
                return false;
            }

            if (image == null)
            {
                if (string.IsNullOrEmpty(failure))
                {
                    failure =
                        "Exact gameBackground UI Image is missing; authored background was left unchanged.";
                }

                return false;
            }

            return true;
        }

        private static void WarnSceneOnce(Scene scene, string message)
        {
            if (warningSceneHandle == scene.handle)
            {
                return;
            }

            warningSceneHandle = scene.handle;
            Debug.LogWarning("[" + PackageId + "] " + message);
        }

        private void Awake()
        {
            if (!string.Equals(
                    gameObject.scene.name,
                    TargetSceneName,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    name,
                    TargetObjectName,
                    StringComparison.Ordinal))
            {
                ReportInstanceWarningOnce(
                    "Controller reached an unauthorized target and was disabled.");
                enabled = false;
                return;
            }

            targetImage = GetComponent<Image>();
            targetRect = transform as RectTransform;
            if (targetImage == null || targetRect == null)
            {
                ReportInstanceWarningOnce(
                    "Target is not the required existing UI Image with RectTransform; authored background was left unchanged.");
                enabled = false;
                return;
            }

            CacheOriginalPresentation();
        }

        private void OnEnable()
        {
            if (targetImage == null || targetRect == null)
            {
                return;
            }

            if (!originalCached)
            {
                CacheOriginalPresentation();
            }

            if (!initialized)
            {
                TryInitialize();
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            float delta = Time.unscaledDeltaTime;
            if (delta <= 0f)
            {
                return;
            }

            phaseElapsed += delta;

            if (playbackPhase == PlaybackPhase.Frames)
            {
                if (phaseElapsed < FrameDurationSeconds)
                {
                    return;
                }

                phaseElapsed -= FrameDurationSeconds;
                if (currentFrameIndex < FrameCount - 1)
                {
                    ApplyFrame(currentFrameIndex + 1, true);
                    return;
                }

                completedSequenceCount++;
                ApplyFrame(0, true);
                playbackPhase = PlaybackPhase.HoldFrameOne;
                phaseElapsed = 0f;
                holdStartedAt = Time.unscaledTimeAsDouble;
                return;
            }

            if (phaseElapsed < HoldDurationSeconds)
            {
                return;
            }

            lastCompletedHoldSeconds =
                (float)(Time.unscaledTimeAsDouble - holdStartedAt);
            minimumCompletedHoldSeconds = Mathf.Min(
                minimumCompletedHoldSeconds,
                lastCompletedHoldSeconds);
            completedHoldCount++;

            // Frame 1 is already visible. Re-applying the same cached wrapper
            // marks the exact start of the next 1->11 sequence without allocation.
            playbackPhase = PlaybackPhase.Frames;
            phaseElapsed = 0f;
            ApplyFrame(0, false);
        }

        private void OnDisable()
        {
            StopAndRestore();
        }

        private void OnDestroy()
        {
            StopAndRestore();
        }

        private void CacheOriginalPresentation()
        {
            originalSprite = targetImage.sprite;
            originalColor = targetImage.color;
            Material resolvedMaterial = targetImage.material;
            originalUsedDefaultMaterial =
                resolvedMaterial == targetImage.defaultMaterial;
            originalMaterial = originalUsedDefaultMaterial
                ? null
                : resolvedMaterial;
            originalRaycastTarget = targetImage.raycastTarget;
            originalPreserveAspect = targetImage.preserveAspect;
            originalImageEnabled = targetImage.enabled;

            originalRect = new RectTransformSnapshot
            {
                AnchorMin = targetRect.anchorMin,
                AnchorMax = targetRect.anchorMax,
                AnchoredPosition = targetRect.anchoredPosition,
                SizeDelta = targetRect.sizeDelta,
                Pivot = targetRect.pivot,
                LocalPosition = targetRect.localPosition,
                LocalScale = targetRect.localScale,
                LocalRotation = targetRect.localRotation,
                Parent = targetRect.parent,
                SiblingIndex = targetRect.GetSiblingIndex()
            };
            originalCached = true;
        }

        private void TryInitialize()
        {
            Texture2D[] loadedTextures =
                Resources.LoadAll<Texture2D>(ResourcePath) ??
                Array.Empty<Texture2D>();
            Dictionary<string, Texture2D> texturesByName =
                new Dictionary<string, Texture2D>(
                    StringComparer.Ordinal);

            for (int i = 0; i < loadedTextures.Length; i++)
            {
                Texture2D texture = loadedTextures[i];
                if (texture == null ||
                    texturesByName.ContainsKey(texture.name))
                {
                    continue;
                }

                texturesByName.Add(texture.name, texture);
            }

            if (loadedTextures.Length != FrameCount ||
                texturesByName.Count != FrameCount)
            {
                FailInitialization(
                    "Expected exactly 11 Texture2D sequence frames; authored background was left unchanged.");
                return;
            }

            Texture2D[] orderedTextures = new Texture2D[FrameCount];
            for (int i = 0; i < FrameCount; i++)
            {
                string expectedName = GetExpectedFrameName(i);
                Texture2D texture;
                if (!texturesByName.TryGetValue(
                        expectedName,
                        out texture) ||
                    texture == null ||
                    texture.width <= 0 ||
                    texture.height <= 0)
                {
                    FailInitialization(
                        "Frame " + expectedName +
                        " is missing or has non-positive imported dimensions; authored background was left unchanged.");
                    return;
                }

                orderedTextures[i] = texture;
            }

            try
            {
                for (int i = 0; i < orderedTextures.Length; i++)
                {
                    Texture2D texture = orderedTextures[i];
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(
                            0f,
                            0f,
                            texture.width,
                            texture.height),
                        new Vector2(0.5f, 0.5f),
                        100f,
                        0,
                        SpriteMeshType.FullRect);
                    if (sprite == null)
                    {
                        throw new InvalidOperationException(
                            "Sprite.Create returned null for " +
                            texture.name);
                    }

                    sprite.name =
                        texture.name + "_RuntimeBackgroundSprite";
                    sprite.hideFlags =
                        HideFlags.DontSaveInEditor |
                        HideFlags.DontSaveInBuild;
                    runtimeSprites.Add(sprite);
                    runtimeSpriteCreationCount++;
                }
            }
            catch (Exception exception)
            {
                FailInitialization(
                    "Runtime Sprite wrapper creation failed (" +
                    exception.GetType().Name +
                    "); authored background was left unchanged.");
                return;
            }

            initialized = true;
            playbackPhase = PlaybackPhase.Frames;
            currentFrameIndex = -1;
            phaseElapsed = 0f;
            completedSequenceCount = 0;
            completedHoldCount = 0;
            frameApplyCount = 0;
            playbackOrderFaultCount = 0;
            expectedFrameIndex = 0;
            lastCompletedHoldSeconds = 0f;
            minimumCompletedHoldSeconds = float.PositiveInfinity;
            ApplyFrame(0, true);

            Debug.Log(
                "[" + PackageId +
                "] Installed on existing gameBackground: 11 cached runtime Sprite wrappers, 10 FPS, 5.0s frame-1 hold; RectTransform untouched.");
        }

        private void ApplyFrame(int frameIndex, bool validateOrder)
        {
            if (!initialized ||
                frameIndex < 0 ||
                frameIndex >= runtimeSprites.Count)
            {
                return;
            }

            if (validateOrder)
            {
                if (frameIndex != expectedFrameIndex)
                {
                    playbackOrderFaultCount++;
                }

                expectedFrameIndex =
                    frameIndex >= FrameCount - 1
                        ? 0
                        : frameIndex + 1;
            }
            else
            {
                expectedFrameIndex = 1;
            }

            targetImage.sprite = runtimeSprites[frameIndex];
            currentFrameIndex = frameIndex;
            frameApplyCount++;
        }

        private void FailInitialization(string message)
        {
            initialized = false;
            RestoreOriginalPresentation();
            DestroyRuntimeSpriteWrappers();
            ReportInstanceWarningOnce(message);
            enabled = false;
        }

        private void StopAndRestore()
        {
            initialized = false;
            RestoreOriginalPresentation();
            DestroyRuntimeSpriteWrappers();
            currentFrameIndex = -1;
            phaseElapsed = 0f;
        }

        private void RestoreOriginalPresentation()
        {
            if (!originalCached || targetImage == null)
            {
                return;
            }

            targetImage.sprite = originalSprite;
            targetImage.color = originalColor;
            targetImage.material =
                originalUsedDefaultMaterial ? null : originalMaterial;
            targetImage.raycastTarget = originalRaycastTarget;
            targetImage.preserveAspect = originalPreserveAspect;
            targetImage.enabled = originalImageEnabled;
        }

        private void DestroyRuntimeSpriteWrappers()
        {
            for (int i = runtimeSprites.Count - 1; i >= 0; i--)
            {
                Sprite sprite = runtimeSprites[i];
                if (sprite != null)
                {
                    Destroy(sprite);
                }
            }

            runtimeSprites.Clear();
        }

        private void ReportInstanceWarningOnce(string message)
        {
            if (instanceWarningReported)
            {
                return;
            }

            instanceWarningReported = true;
            Debug.LogWarning("[" + PackageId + "] " + message);
        }

        private bool MatchesAuthoredGeometry()
        {
            return originalCached &&
                targetRect != null &&
                targetRect.anchorMin == originalRect.AnchorMin &&
                targetRect.anchorMax == originalRect.AnchorMax &&
                targetRect.anchoredPosition ==
                    originalRect.AnchoredPosition &&
                targetRect.sizeDelta == originalRect.SizeDelta &&
                targetRect.pivot == originalRect.Pivot &&
                targetRect.localPosition == originalRect.LocalPosition &&
                targetRect.localScale == originalRect.LocalScale &&
                targetRect.localRotation == originalRect.LocalRotation &&
                targetRect.parent == originalRect.Parent &&
                targetRect.GetSiblingIndex() ==
                    originalRect.SiblingIndex;
        }

        private bool MatchesOriginalNonSpritePresentation()
        {
            if (!originalCached || targetImage == null)
            {
                return false;
            }

            Material resolvedMaterial = targetImage.material;
            bool materialMatches = originalUsedDefaultMaterial
                ? resolvedMaterial == targetImage.defaultMaterial
                : resolvedMaterial == originalMaterial;

            return targetImage.color == originalColor &&
                materialMatches &&
                targetImage.raycastTarget == originalRaycastTarget &&
                targetImage.preserveAspect == originalPreserveAspect &&
                targetImage.enabled == originalImageEnabled;
        }

        private bool MatchesFullyRestoredPresentation()
        {
            return MatchesAuthoredGeometry() &&
                MatchesOriginalNonSpritePresentation() &&
                targetImage.sprite == originalSprite;
        }

        private static string GetExpectedFrameName(int index)
        {
            return "frame_" + (index + 1).ToString("00000");
        }

#if UNITY_EDITOR
        private const string LeaseQaScenePath =
            "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
        private const string LeaseQaPendingKey =
            "TalismanBag.AnimatedBackground.LeaseQaPending";
        private const string LeaseQaExitKey =
            "TalismanBag.AnimatedBackground.LeaseQaExit";
        private const string LeaseQaPassedKey =
            "TalismanBag.AnimatedBackground.LeaseQaPassed";
        private const string LeaseQaResultKey =
            "TalismanBag.AnimatedBackground.LeaseQaResult";
        private const string LeaseQaBaselineKey =
            "TalismanBag.AnimatedBackground.LeaseQaBaseline";
        private const string LeaseQaCaptureDirectoryKey =
            "TalismanBag.AnimatedBackground.LeaseQaCaptureDirectory";
        private const string LeaseQaCaptureArgument =
            "-backgroundQaCaptureDir";

        private static int leaseQaInstallAttempts;

        [Serializable]
        private sealed class EditorTargetBaseline
        {
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 AnchoredPosition;
            public Vector2 SizeDelta;
            public Vector2 Pivot;
            public Vector3 LocalPosition;
            public Vector3 LocalScale;
            public Quaternion LocalRotation;
            public int SiblingIndex;
            public string ParentPath;
            public bool ImageEnabled;
            public Color ImageColor;
            public bool RaycastTarget;
            public bool PreserveAspect;
            public string SpriteAssetKey;
            public string MaterialAssetKey;
        }

        [InitializeOnLoadMethod]
        private static void RegisterLeaseQaEditorHooks()
        {
            EditorApplication.playModeStateChanged -=
                HandleLeaseQaPlayModeStateChanged;
            EditorApplication.playModeStateChanged +=
                HandleLeaseQaPlayModeStateChanged;
        }

        public static void RunLeaseQaBatch()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                throw new InvalidOperationException(
                    PackageId + " lease QA requires Edit Mode.");
            }

            Scene scene = EditorSceneManager.OpenScene(
                LeaseQaScenePath,
                OpenSceneMode.Single);
            if (!scene.IsValid() ||
                !string.Equals(
                    scene.path,
                    LeaseQaScenePath,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    PackageId +
                    " could not open the authorized sandbox scene.");
            }

            Image image;
            string failure;
            if (!TryFindExactTarget(scene, out image, out failure))
            {
                throw new InvalidOperationException(
                    PackageId + " pre-Play target check failed: " +
                    failure);
            }

            EditorTargetBaseline baseline =
                CaptureEditorTargetBaseline(image);
            string captureDirectory =
                ReadCommandLineArgument(LeaseQaCaptureArgument);
            if (string.IsNullOrWhiteSpace(captureDirectory))
            {
                throw new InvalidOperationException(
                    PackageId + " requires " +
                    LeaseQaCaptureArgument +
                    " for rendered QA.");
            }

            SessionState.SetBool(LeaseQaPendingKey, true);
            SessionState.SetBool(LeaseQaExitKey, false);
            SessionState.SetBool(LeaseQaPassedKey, false);
            SessionState.SetString(LeaseQaResultKey, string.Empty);
            SessionState.SetString(
                LeaseQaBaselineKey,
                JsonUtility.ToJson(baseline));
            SessionState.SetString(
                LeaseQaCaptureDirectoryKey,
                Path.GetFullPath(captureDirectory));
            leaseQaInstallAttempts = 0;

            Debug.Log(
                "[" + PackageId +
                "] BACKGROUND_SEQUENCE_QA_ENTER_PLAY scene=" +
                LeaseQaScenePath);
            EditorApplication.EnterPlaymode();
        }

        private static void HandleLeaseQaPlayModeStateChanged(
            PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode &&
                SessionState.GetBool(LeaseQaPendingKey, false))
            {
                SessionState.EraseBool(LeaseQaPendingKey);
                leaseQaInstallAttempts = 0;
                EditorApplication.delayCall +=
                    InstallLeaseQaRuntimeDriver;
                return;
            }

            if (state != PlayModeStateChange.EnteredEditMode ||
                !SessionState.GetBool(LeaseQaExitKey, false))
            {
                return;
            }

            bool passed =
                SessionState.GetBool(LeaseQaPassedKey, false);
            string result = SessionState.GetString(
                LeaseQaResultKey,
                "BACKGROUND_SEQUENCE_QA_FAIL missing runtime result");

            string restorationFailure;
            if (!ValidateEditorRestoration(out restorationFailure))
            {
                passed = false;
                result += "; editorRestore=FAIL(" +
                    restorationFailure + ")";
            }
            else
            {
                result +=
                    "; editorRestore=pass; sceneDirty=false";
            }

            SessionState.EraseBool(LeaseQaExitKey);
            SessionState.EraseBool(LeaseQaPassedKey);
            SessionState.EraseString(LeaseQaResultKey);
            SessionState.EraseString(LeaseQaBaselineKey);
            SessionState.EraseString(
                LeaseQaCaptureDirectoryKey);

            if (passed)
            {
                Debug.Log("[" + PackageId + "] " + result);
            }
            else
            {
                Debug.LogError("[" + PackageId + "] " + result);
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static void InstallLeaseQaRuntimeDriver()
        {
            if (!EditorApplication.isPlaying)
            {
                CompleteLeaseQaFromPlay(
                    false,
                    "BACKGROUND_SEQUENCE_QA_FAIL editor left Play Mode before driver installation");
                return;
            }

            BattleSandboxAnimatedBackgroundPrototypeController
                controller =
                    FindObjectOfType<
                        BattleSandboxAnimatedBackgroundPrototypeController>();
            if (controller == null || !controller.initialized)
            {
                leaseQaInstallAttempts++;
                if (leaseQaInstallAttempts < 240)
                {
                    EditorApplication.delayCall +=
                        InstallLeaseQaRuntimeDriver;
                    return;
                }

                CompleteLeaseQaFromPlay(
                    false,
                    "BACKGROUND_SEQUENCE_QA_FAIL runtime controller was not auto-attached within 240 editor updates");
                return;
            }

            GameObject driverObject = new GameObject(
                "BattleSandboxAnimatedBackground_LeaseQaRuntimeDriver");
            driverObject.hideFlags = HideFlags.HideAndDontSave;
            LeaseQaRuntimeDriver driver =
                driverObject.AddComponent<LeaseQaRuntimeDriver>();
            driver.Begin(controller);
        }

        private static void CompleteLeaseQaFromPlay(
            bool passed,
            string result)
        {
            SessionState.SetBool(LeaseQaPassedKey, passed);
            SessionState.SetString(LeaseQaResultKey, result);
            SessionState.SetBool(LeaseQaExitKey, true);
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
            else if (Application.isBatchMode)
            {
                EditorApplication.Exit(passed ? 0 : 1);
            }
        }

        private static EditorTargetBaseline
            CaptureEditorTargetBaseline(Image image)
        {
            RectTransform rect = image.rectTransform;
            SerializedObject serializedImage =
                new SerializedObject(image);
            UnityEngine.Object sprite =
                serializedImage.FindProperty(
                    "m_Sprite").objectReferenceValue;
            UnityEngine.Object material =
                serializedImage.FindProperty(
                    "m_Material").objectReferenceValue;

            return new EditorTargetBaseline
            {
                AnchorMin = rect.anchorMin,
                AnchorMax = rect.anchorMax,
                AnchoredPosition = rect.anchoredPosition,
                SizeDelta = rect.sizeDelta,
                Pivot = rect.pivot,
                LocalPosition = rect.localPosition,
                LocalScale = rect.localScale,
                LocalRotation = rect.localRotation,
                SiblingIndex = rect.GetSiblingIndex(),
                ParentPath = rect.parent == null
                    ? string.Empty
                    : AnimationUtility.CalculateTransformPath(
                        rect.parent,
                        null),
                ImageEnabled = image.enabled,
                ImageColor = image.color,
                RaycastTarget = image.raycastTarget,
                PreserveAspect = image.preserveAspect,
                SpriteAssetKey = GetEditorAssetKey(sprite),
                MaterialAssetKey = GetEditorAssetKey(material)
            };
        }

        private static bool ValidateEditorRestoration(
            out string failure)
        {
            failure = string.Empty;
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() ||
                !string.Equals(
                    scene.path,
                    LeaseQaScenePath,
                    StringComparison.Ordinal))
            {
                failure = "authorized scene is no longer active";
                return false;
            }

            if (scene.isDirty)
            {
                failure = "scene became dirty";
                return false;
            }

            Image image;
            if (!TryFindExactTarget(scene, out image, out failure))
            {
                return false;
            }

            string json = SessionState.GetString(
                LeaseQaBaselineKey,
                string.Empty);
            EditorTargetBaseline baseline =
                JsonUtility.FromJson<EditorTargetBaseline>(json);
            if (baseline == null)
            {
                failure = "editor baseline is missing";
                return false;
            }

            EditorTargetBaseline actual =
                CaptureEditorTargetBaseline(image);
            if (!EditorBaselinesEqual(baseline, actual))
            {
                failure =
                    "Image or RectTransform differs from the pre-Play authored baseline";
                return false;
            }

            if (image.GetComponent<
                    BattleSandboxAnimatedBackgroundPrototypeController>() !=
                null)
            {
                failure =
                    "runtime-only controller remained after leaving Play";
                return false;
            }

            return true;
        }

        private static bool EditorBaselinesEqual(
            EditorTargetBaseline expected,
            EditorTargetBaseline actual)
        {
            return expected.AnchorMin == actual.AnchorMin &&
                expected.AnchorMax == actual.AnchorMax &&
                expected.AnchoredPosition ==
                    actual.AnchoredPosition &&
                expected.SizeDelta == actual.SizeDelta &&
                expected.Pivot == actual.Pivot &&
                expected.LocalPosition == actual.LocalPosition &&
                expected.LocalScale == actual.LocalScale &&
                expected.LocalRotation == actual.LocalRotation &&
                expected.SiblingIndex == actual.SiblingIndex &&
                string.Equals(
                    expected.ParentPath,
                    actual.ParentPath,
                    StringComparison.Ordinal) &&
                expected.ImageEnabled == actual.ImageEnabled &&
                expected.ImageColor == actual.ImageColor &&
                expected.RaycastTarget == actual.RaycastTarget &&
                expected.PreserveAspect == actual.PreserveAspect &&
                string.Equals(
                    expected.SpriteAssetKey,
                    actual.SpriteAssetKey,
                    StringComparison.Ordinal) &&
                string.Equals(
                    expected.MaterialAssetKey,
                    actual.MaterialAssetKey,
                    StringComparison.Ordinal);
        }

        private static string GetEditorAssetKey(
            UnityEngine.Object asset)
        {
            if (asset == null)
            {
                return "<null>";
            }

            string guid;
            long localId;
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(
                    asset,
                    out guid,
                    out localId))
            {
                return "<nonasset>:" + asset.GetInstanceID();
            }

            return guid + ":" +
                localId.ToString(
                    CultureInfo.InvariantCulture) +
                ":" + AssetDatabase.GetAssetPath(asset);
        }

        private static string ReadCommandLineArgument(
            string argumentName)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int i = 0; i < arguments.Length - 1; i++)
            {
                if (string.Equals(
                        arguments[i],
                        argumentName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[i + 1];
                }
            }

            return string.Empty;
        }

        private sealed class LeaseQaRuntimeDriver : MonoBehaviour
        {
            private readonly List<string> failures =
                new List<string>();
            private readonly List<string> packageWarnings =
                new List<string>();
            private readonly List<string> packageErrors =
                new List<string>();

            private BattleSandboxAnimatedBackgroundPrototypeController
                controller;
            private string captureDirectory;
            private string holdCapturePath;
            private string laterCapturePath;
            private bool completing;

            public void Begin(
                BattleSandboxAnimatedBackgroundPrototypeController
                    target)
            {
                controller = target;
                captureDirectory = SessionState.GetString(
                    LeaseQaCaptureDirectoryKey,
                    string.Empty);
                Application.logMessageReceived += CaptureLog;
                StartCoroutine(Run());
            }

            private IEnumerator Run()
            {
                Require(
                    controller != null &&
                    controller.initialized,
                    "runtime controller is not initialized");
                Require(
                    controller.gameObject.scene.name ==
                        TargetSceneName &&
                    controller.name == TargetObjectName,
                    "runtime controller is attached to an unauthorized target");
                Require(
                    controller.runtimeSprites.Count == FrameCount &&
                    controller.runtimeSpriteCreationCount ==
                        FrameCount,
                    "runtime Sprite wrappers were not created exactly once");
                Require(
                    controller.currentFrameIndex == 0,
                    "automatic playback did not start at frame 1");
                Require(
                    controller.MatchesAuthoredGeometry(),
                    "RectTransform or hierarchy changed during startup");
                Require(
                    controller.MatchesOriginalNonSpritePresentation(),
                    "Image non-sprite presentation changed during startup");

                for (int i = 0; i < FrameCount; i++)
                {
                    Sprite sprite = controller.runtimeSprites[i];
                    string expectedName =
                        GetExpectedFrameName(i) +
                        "_RuntimeBackgroundSprite";
                    Require(
                        sprite != null &&
                        string.Equals(
                            sprite.name,
                            expectedName,
                            StringComparison.Ordinal) &&
                        sprite.texture != null &&
                        sprite.texture.width > 0 &&
                        sprite.texture.height > 0,
                        "numeric frame order or dimensions failed at " +
                        (i + 1).ToString(
                            CultureInfo.InvariantCulture));
                }

                try
                {
                    Directory.CreateDirectory(captureDirectory);
                    holdCapturePath = Path.Combine(
                        captureDirectory,
                        "BattleSandbox_Background_Frame01_Hold.png");
                    laterCapturePath = Path.Combine(
                        captureDirectory,
                        "BattleSandbox_Background_Frame10.png");
                    DeleteExistingCapture(holdCapturePath);
                    DeleteExistingCapture(laterCapturePath);
                }
                catch (Exception exception)
                {
                    failures.Add(
                        "capture directory failed: " +
                        exception.GetType().Name);
                }

                if (failures.Count > 0)
                {
                    Finish();
                    yield break;
                }

                double deadline =
                    Time.unscaledTimeAsDouble + 4.0;
                while (Time.unscaledTimeAsDouble < deadline &&
                    !(controller.IsHoldingFrameOne &&
                      controller.CompletedSequenceCount >= 1))
                {
                    yield return null;
                }

                Require(
                    controller.IsHoldingFrameOne &&
                    controller.CurrentFrameIndex == 0 &&
                    controller.CompletedSequenceCount >= 1,
                    "first frame-1 hold was not reached");
                if (failures.Count == 0)
                {
                    yield return CaptureFrame(holdCapturePath);
                }

                deadline = Time.unscaledTimeAsDouble + 7.0;
                while (Time.unscaledTimeAsDouble < deadline &&
                    controller.CompletedHoldCount < 1)
                {
                    yield return null;
                }

                Require(
                    controller.CompletedHoldCount >= 1,
                    "first 5-second hold did not complete");

                deadline = Time.unscaledTimeAsDouble + 3.0;
                while (Time.unscaledTimeAsDouble < deadline &&
                    !(controller.CompletedSequenceCount == 1 &&
                      !controller.IsHoldingFrameOne &&
                      controller.CurrentFrameIndex == 9))
                {
                    yield return null;
                }

                Require(
                    controller.CompletedSequenceCount == 1 &&
                    !controller.IsHoldingFrameOne &&
                    controller.CurrentFrameIndex == 9,
                    "clearly different frame 10 was not observed in loop 2");
                if (failures.Count == 0)
                {
                    yield return CaptureFrame(laterCapturePath);
                }

                deadline = Time.unscaledTimeAsDouble + 8.0;
                while (Time.unscaledTimeAsDouble < deadline &&
                    controller.CompletedHoldCount < 2)
                {
                    yield return null;
                }

                Require(
                    controller.CompletedSequenceCount >= 2 &&
                    controller.CompletedHoldCount >= 2,
                    "two complete sequence+hold loops were not observed");
                Require(
                    controller.MinimumCompletedHoldSeconds >= 4.98f,
                    "measured frame-1 hold was shorter than 5.0 seconds within frame tolerance");
                Require(
                    controller.runtimeSpriteCreationCount ==
                        FrameCount &&
                    controller.runtimeSprites.Count == FrameCount,
                    "Sprite wrappers were allocated again per loop");
                Require(
                    controller.playbackOrderFaultCount == 0,
                    "playback order fault detected");
                Require(
                    controller.MatchesAuthoredGeometry(),
                    "RectTransform or hierarchy drifted during loops");
                Require(
                    controller.MatchesOriginalNonSpritePresentation(),
                    "Image non-sprite presentation drifted during loops");
                Require(
                    File.Exists(holdCapturePath) &&
                    new FileInfo(holdCapturePath).Length > 0,
                    "frame-1 hold screenshot was not written");
                Require(
                    File.Exists(laterCapturePath) &&
                    new FileInfo(laterCapturePath).Length > 0,
                    "later-frame screenshot was not written");

                int createdBeforeDisable =
                    controller.runtimeSpriteCreationCount;
                controller.enabled = false;
                yield return null;

                Require(
                    createdBeforeDisable == FrameCount &&
                    controller.runtimeSprites.Count == 0,
                    "package-created Sprite wrappers were not cleaned up");
                Require(
                    controller.MatchesFullyRestoredPresentation(),
                    "authored Image presentation was not restored on disable");
                Require(
                    packageWarnings.Count == 0,
                    "package emitted warning(s) during normal QA");
                Require(
                    packageErrors.Count == 0,
                    "package emitted error(s) during normal QA");

                Finish();
            }

            private IEnumerator CaptureFrame(string path)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(path);

                double deadline =
                    Time.unscaledTimeAsDouble + 4.0;
                while (Time.unscaledTimeAsDouble < deadline)
                {
                    if (File.Exists(path) &&
                        new FileInfo(path).Length > 0)
                    {
                        yield break;
                    }

                    yield return null;
                }
            }

            private void Require(bool condition, string failure)
            {
                if (!condition)
                {
                    failures.Add(failure);
                }
            }

            private void Finish()
            {
                if (completing)
                {
                    return;
                }

                completing = true;
                Application.logMessageReceived -= CaptureLog;
                bool passed = failures.Count == 0;
                string result;
                if (passed)
                {
                    result =
                        "BACKGROUND_SEQUENCE_QA_PASS" +
                        "; frames=11" +
                        "; order=frame_00001..frame_00011" +
                        "; fps=10" +
                        "; sequences=" +
                        controller.CompletedSequenceCount +
                        "; holds=" +
                        controller.CompletedHoldCount +
                        "; minHold=" +
                        controller.MinimumCompletedHoldSeconds.ToString(
                            "F3",
                            CultureInfo.InvariantCulture) +
                        "s" +
                        "; spriteAllocations=" +
                        controller.RuntimeSpriteCreationCount +
                        "; perLoopSpriteAllocations=0" +
                        "; rectTransform=unchanged" +
                        "; imagePresentation=restoredOnDisable" +
                        "; packageWarnings=0" +
                        "; packageErrors=0" +
                        "; frame01Capture=" + holdCapturePath +
                        "; frame10Capture=" + laterCapturePath;
                }
                else
                {
                    result =
                        "BACKGROUND_SEQUENCE_QA_FAIL " +
                        string.Join(" | ", failures);
                }

                CompleteLeaseQaFromPlay(passed, result);
            }

            private void CaptureLog(
                string condition,
                string stackTrace,
                LogType type)
            {
                if (string.IsNullOrEmpty(condition) ||
                    condition.IndexOf(
                        PackageId,
                        StringComparison.Ordinal) < 0)
                {
                    return;
                }

                if (type == LogType.Warning)
                {
                    packageWarnings.Add(condition);
                }
                else if (type == LogType.Error ||
                    type == LogType.Exception ||
                    type == LogType.Assert)
                {
                    packageErrors.Add(condition);
                }
            }

            private static void DeleteExistingCapture(string path)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            private void OnDestroy()
            {
                Application.logMessageReceived -= CaptureLog;
            }
        }
#endif
    }
}
