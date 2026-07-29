using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public enum ShougunuPhase1PresentationAction
{
    Idle,
    BasicAttack,
    Hit,
    Skill1,
    Skill2,
    Skill3,
    ShellBreak,
    Defeated
}

/// <summary>
/// Dev-only presentation prototype for Shougunu phase one.
/// It owns no battle truth and writes no scene state. Every motion sample is
/// evaluated from a cached neutral pose so interrupted or repeated actions cannot drift.
/// </summary>
[DisallowMultipleComponent]
public sealed class ShougunuPhase1VisualPrototypeController : MonoBehaviour
{
    private const string PackageId = "V0.4-ShougunuPhase1FullAnimationVfxPrototype01";
    private const string TargetSceneName = "Scene_TalismanBag_V04_BattleSandboxPreview";
    private const string TargetObjectName = "Shougunu_1";
    private const string TargetParentName = "V02EnemyArea";
    private const string ResourceRoot = "Enemy/守骨奴 第一阶段状态/";
    private const string AdditiveShaderName = "TalismanBag/UI/CellGlowAdditive";

    private enum PoseKind
    {
        Idle,
        Attack,
        Hit,
        Skill1,
        Skill2,
        Skill3
    }

    private enum ActionKind
    {
        Idle,
        Attack,
        Hit,
        Skill1,
        Skill2,
        Skill3
    }

    private readonly struct PoseCalibration
    {
        public readonly PoseKind Kind;
        public readonly string ResourceName;
        public readonly Vector2 Offset;
        public readonly float Scale;
        public readonly int FootPixelFromTop;

        public PoseCalibration(
            PoseKind kind,
            string resourceName,
            Vector2 horizontalAndAuthoredOffset,
            float scale,
            int footPixelFromTop)
        {
            Kind = kind;
            ResourceName = resourceName;
            Scale = scale;
            FootPixelFromTop = footPixelFromTop;

            // The source sheets are all 1700x1500. 1316 is the observed idle foot line.
            // Scaling is applied around the sheet centre, then this correction restores
            // the same authoritative local-space foot line for every pose.
            const float halfHeight = 750f;
            const float neutralFootLocalY = halfHeight - 1316f;
            float sourceFootLocalY = halfHeight - footPixelFromTop;
            float footCorrectionY = neutralFootLocalY - sourceFootLocalY * scale;
            Offset = new Vector2(horizontalAndAuthoredOffset.x, footCorrectionY + horizontalAndAuthoredOffset.y);
        }
    }

    private sealed class PoseAsset
    {
        public PoseCalibration Calibration;
        public Sprite Sprite;
        public bool UsedFallback;
    }

    private sealed class AmbientMote
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 BasePosition;
        public float Phase;
        public float Radius;
    }

    private static readonly PoseCalibration[] Calibrations =
    {
        new PoseCalibration(PoseKind.Idle, "守骨奴 第一阶段_idle", new Vector2(0f, 0f), 1f, 1316),
        new PoseCalibration(PoseKind.Attack, "守骨奴 第一阶段_attack", new Vector2(76f, 0f), 0.98f, 1372),
        new PoseCalibration(PoseKind.Hit, "守骨奴 第一阶段_hit", new Vector2(-24f, 0f), 1f, 1316),
        new PoseCalibration(PoseKind.Skill1, "守骨奴 第一阶段_skill_1", new Vector2(-10f, 0f), 0.96f, 1316),
        new PoseCalibration(PoseKind.Skill2, "守骨奴 第一阶段_skill_2", new Vector2(4f, 0f), 0.94f, 1312),
        new PoseCalibration(PoseKind.Skill3, "守骨奴 第一阶段_skill_3", new Vector2(-6f, 0f), 0.95f, 1340)
    };

    private static bool bootstrapRegistered;

    private readonly Dictionary<PoseKind, PoseAsset> poses = new Dictionary<PoseKind, PoseAsset>();
    private readonly List<GameObject> transientObjects = new List<GameObject>();
    private readonly List<Sprite> ownedSprites = new List<Sprite>();
    private readonly List<Texture2D> ownedTextures = new List<Texture2D>();
    private readonly List<Material> ownedMaterials = new List<Material>();
    private readonly List<AmbientMote> ambientMotes = new List<AmbientMote>();

    private Image sourceImage;
    private Sprite originalSprite;
    private Material originalMaterial;
    private Color originalColor;
    private bool originalEnabled;
    private bool originalPreserveAspect;
    private bool originalRaycastTarget;

    private RectTransform targetRect;
    private RectTransform backRoot;
    private RectTransform presenceRoot;
    private RectTransform motionRoot;
    private RectTransform poseRoot;
    private RectTransform frontRoot;
    private Image bodyImage;
    private Image bodyFlashImage;
    private CanvasGroup presenceGroup;
    private RectTransform presenceOuterRing;
    private RectTransform presenceInnerRing;
    private RectTransform presenceGlow;
    private Image presenceOuterRingImage;
    private Image presenceInnerRingImage;
    private Image presenceGlowImage;

    private GameObject controlPanel;
    private Text statusText;
    private Sprite solidSprite;
    private Sprite softCircleSprite;
    private Sprite ringSprite;
    private Sprite flameSprite;
    private Material additiveMaterial;

    private Vector2 neutralMotionPosition;
    private Vector3 neutralMotionScale;
    private Quaternion neutralMotionRotation;
    private PoseKind currentPose = PoseKind.Idle;
    private bool initialized;
    private bool actionActive;
    private bool autoActive;
    private bool stressActive;
    private bool defeatedPresentationActive;
    private bool lastStressPassed;
    private int autoPassCount;
    private float playbackSpeed = 1f;
    private int actionSerial;

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
        TryAttach(scene);
    }

    private static void TryAttach(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded || scene.name != TargetSceneName)
        {
            return;
        }

        Image match = null;
        Image[] images = FindObjectsOfType<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image candidate = images[i];
            if (candidate == null ||
                candidate.gameObject.scene != scene ||
                candidate.name != TargetObjectName ||
                candidate.transform.parent == null ||
                candidate.transform.parent.name != TargetParentName)
            {
                continue;
            }

            if (match != null)
            {
                Debug.LogWarning($"[{PackageId}] More than one exact {TargetParentName}/{TargetObjectName} match exists; no controller was attached.");
                return;
            }

            match = candidate;
        }

        if (match == null)
        {
            Debug.LogWarning($"[{PackageId}] Exact runtime target {TargetParentName}/{TargetObjectName} was not found. Scene remains untouched.");
            return;
        }

        if (match.GetComponent<ShougunuPhase1VisualPrototypeController>() == null)
        {
            match.gameObject.AddComponent<ShougunuPhase1VisualPrototypeController>();
        }
    }

    private void Awake()
    {
        if (gameObject.scene.name != TargetSceneName ||
            name != TargetObjectName ||
            transform.parent == null ||
            transform.parent.name != TargetParentName)
        {
            enabled = false;
            return;
        }

        sourceImage = GetComponent<Image>();
        targetRect = transform as RectTransform;
        if (sourceImage == null || targetRect == null)
        {
            Debug.LogWarning($"[{PackageId}] Target is not a UI Image with RectTransform; prototype disabled.");
            enabled = false;
            return;
        }

        CacheSourceState();
        CreateGeneratedResources();
        LoadPoseAssets();

        if (!poses.TryGetValue(PoseKind.Idle, out PoseAsset idle) || idle.Sprite == null)
        {
            Debug.LogWarning($"[{PackageId}] No phase-one texture could be loaded. Original Image was preserved.");
            enabled = false;
            return;
        }

        BuildRuntimeHierarchy();
        BuildControls();
        sourceImage.enabled = false;
        initialized = true;
        ResetToAuthoritativeNeutral("Ready — 1–6 actions, Space auto, F9 ×20 stability");
        Debug.Log($"[{PackageId}] Runtime prototype attached to {TargetParentName}/{TargetObjectName}; scene data was not modified.");
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartManual(ActionKind.Idle);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartManual(ActionKind.Attack);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            StartManual(ActionKind.Hit);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            StartManual(ActionKind.Skill1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            StartManual(ActionKind.Skill2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            StartManual(ActionKind.Skill3);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleAutoShowcase();
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            StartStabilityStressProbe();
        }

        UpdatePresence(Time.unscaledTime);
    }

    /// <summary>
    /// Presentation-only command facade. A true result means only that this
    /// visual controller could play the cue; it is never combat acceptance.
    /// </summary>
    public bool TryPlayPresentationAction(
        ShougunuPhase1PresentationAction action)
    {
        if (!initialized || !isActiveAndEnabled)
        {
            return false;
        }

        if (defeatedPresentationActive
            && action != ShougunuPhase1PresentationAction.Idle)
        {
            return false;
        }

        switch (action)
        {
            case ShougunuPhase1PresentationAction.Idle:
                StopPlaybackAndReset();
                SetStatus("Idle / Presentation facade reset");
                return true;
            case ShougunuPhase1PresentationAction.BasicAttack:
                StartManual(ActionKind.Attack);
                return true;
            case ShougunuPhase1PresentationAction.Hit:
                StartManual(ActionKind.Hit);
                return true;
            case ShougunuPhase1PresentationAction.Skill1:
                StartManual(ActionKind.Skill1);
                return true;
            case ShougunuPhase1PresentationAction.Skill2:
                StartManual(ActionKind.Skill2);
                return true;
            case ShougunuPhase1PresentationAction.Skill3:
                StartManual(ActionKind.Skill3);
                return true;
            case ShougunuPhase1PresentationAction.ShellBreak:
                StartShellBreakPresentation();
                return true;
            case ShougunuPhase1PresentationAction.Defeated:
                StartDefeatedPresentation();
                return true;
            default:
                return false;
        }
    }

    private void OnEnable()
    {
        if (!initialized)
        {
            return;
        }

        SetRuntimePresentationActive(true);
        sourceImage.enabled = false;
        ResetToAuthoritativeNeutral("Ready — 1–6 actions, Space auto, F9 ×20 stability");
    }

    private void OnDisable()
    {
        if (initialized)
        {
            StopAllCoroutines();
            ClearTransientObjects();
            SetRuntimePresentationActive(false);
            RestoreSourceState();
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        ClearTransientObjects();
        RestoreSourceState();

        if (controlPanel != null)
        {
            Destroy(controlPanel);
        }

        for (int i = 0; i < ownedMaterials.Count; i++)
        {
            if (ownedMaterials[i] != null)
            {
                Destroy(ownedMaterials[i]);
            }
        }

        for (int i = 0; i < ownedSprites.Count; i++)
        {
            if (ownedSprites[i] != null)
            {
                Destroy(ownedSprites[i]);
            }
        }

        for (int i = 0; i < ownedTextures.Count; i++)
        {
            if (ownedTextures[i] != null)
            {
                Destroy(ownedTextures[i]);
            }
        }
    }

    private void SetRuntimePresentationActive(bool active)
    {
        if (backRoot != null)
        {
            backRoot.gameObject.SetActive(active);
        }

        if (motionRoot != null)
        {
            motionRoot.gameObject.SetActive(active);
        }

        if (frontRoot != null)
        {
            frontRoot.gameObject.SetActive(active);
        }

        if (controlPanel != null)
        {
            controlPanel.SetActive(active);
        }
    }

    private void CacheSourceState()
    {
        originalSprite = sourceImage.sprite;
        originalMaterial = sourceImage.material;
        originalColor = sourceImage.color;
        originalEnabled = sourceImage.enabled;
        originalPreserveAspect = sourceImage.preserveAspect;
        originalRaycastTarget = sourceImage.raycastTarget;
    }

    private void RestoreSourceState()
    {
        if (sourceImage == null)
        {
            return;
        }

        sourceImage.sprite = originalSprite;
        sourceImage.material = originalMaterial;
        sourceImage.color = originalColor;
        sourceImage.preserveAspect = originalPreserveAspect;
        sourceImage.raycastTarget = originalRaycastTarget;
        sourceImage.enabled = originalEnabled;
    }

    private void CreateGeneratedResources()
    {
        solidSprite = CreateGeneratedSprite("Shougunu_Solid", 4, (x, y, size) => Color.white);
        softCircleSprite = CreateGeneratedSprite("Shougunu_SoftCircle", 96, (x, y, size) =>
        {
            float nx = (x + 0.5f) / size * 2f - 1f;
            float ny = (y + 0.5f) / size * 2f - 1f;
            float distance = Mathf.Sqrt(nx * nx + ny * ny);
            float alpha = 1f - Mathf.SmoothStep(0.05f, 1f, distance);
            return new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
        });
        ringSprite = CreateGeneratedSprite("Shougunu_Ring", 128, (x, y, size) =>
        {
            float nx = (x + 0.5f) / size * 2f - 1f;
            float ny = (y + 0.5f) / size * 2f - 1f;
            float distance = Mathf.Sqrt(nx * nx + ny * ny);
            float band = 1f - Mathf.Clamp01(Mathf.Abs(distance - 0.76f) / 0.055f);
            float outside = 1f - Mathf.SmoothStep(0.92f, 1f, distance);
            return new Color(1f, 1f, 1f, band * outside);
        });
        flameSprite = CreateGeneratedSprite("Shougunu_Flame", 96, (x, y, size) =>
        {
            float u = (x + 0.5f) / size;
            float v = (y + 0.5f) / size;
            float centre = 0.5f + Mathf.Sin(v * 10.5f) * 0.035f * v;
            float halfWidth = Mathf.Lerp(0.36f, 0.025f, Mathf.Pow(v, 0.72f));
            float edge = 1f - Mathf.Clamp01(Mathf.Abs(u - centre) / Mathf.Max(0.001f, halfWidth));
            float baseFade = Mathf.SmoothStep(0f, 0.12f, v);
            float tipFade = 1f - Mathf.SmoothStep(0.82f, 1f, v);
            return new Color(1f, 1f, 1f, edge * edge * baseFade * tipFade);
        });

        Shader additiveShader = Shader.Find(AdditiveShaderName);
        if (additiveShader != null)
        {
            additiveMaterial = new Material(additiveShader)
            {
                name = "Shougunu_Runtime_UI_Additive",
                hideFlags = HideFlags.DontSave
            };
            ownedMaterials.Add(additiveMaterial);
        }
        else
        {
            Debug.LogWarning($"[{PackageId}] Optional UI additive shader was unavailable; VFX safely falls back to alpha blending.");
        }
    }

    private Sprite CreateGeneratedSprite(
        string assetName,
        int size,
        Func<int, int, int, Color> pixelFactory)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = assetName + "_Texture",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = pixelFactory(x, y, size);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        ownedTextures.Add(texture);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect);
        sprite.name = assetName;
        sprite.hideFlags = HideFlags.DontSave;
        ownedSprites.Add(sprite);
        return sprite;
    }

    private void LoadPoseAssets()
    {
        Sprite firstAvailable = null;
        for (int i = 0; i < Calibrations.Length; i++)
        {
            PoseCalibration calibration = Calibrations[i];
            Texture2D texture = Resources.Load<Texture2D>(ResourceRoot + calibration.ResourceName);
            if (texture == null)
            {
                continue;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "Shougunu_Runtime_" + calibration.Kind;
            sprite.hideFlags = HideFlags.DontSave;
            ownedSprites.Add(sprite);
            if (firstAvailable == null)
            {
                firstAvailable = sprite;
            }

            poses[calibration.Kind] = new PoseAsset
            {
                Calibration = calibration,
                Sprite = sprite,
                UsedFallback = false
            };
        }

        if (!poses.TryGetValue(PoseKind.Idle, out PoseAsset idleAsset) || idleAsset.Sprite == null)
        {
            if (firstAvailable != null)
            {
                PoseCalibration idleCalibration = Calibrations[0];
                poses[PoseKind.Idle] = new PoseAsset
                {
                    Calibration = idleCalibration,
                    Sprite = firstAvailable,
                    UsedFallback = true
                };
                Debug.LogWarning($"[{PackageId}] Idle texture missing; first available phase-one texture is used as a safe fallback.");
            }
        }

        Sprite fallback = poses.TryGetValue(PoseKind.Idle, out PoseAsset resolvedIdle)
            ? resolvedIdle.Sprite
            : firstAvailable;
        for (int i = 0; i < Calibrations.Length; i++)
        {
            PoseCalibration calibration = Calibrations[i];
            if (poses.ContainsKey(calibration.Kind))
            {
                continue;
            }

            poses[calibration.Kind] = new PoseAsset
            {
                Calibration = calibration,
                Sprite = fallback,
                UsedFallback = true
            };
            Debug.LogWarning($"[{PackageId}] {calibration.ResourceName}.png missing; {calibration.Kind} keeps procedural VFX and uses the safe idle fallback.");
        }
    }

    private void BuildRuntimeHierarchy()
    {
        backRoot = CreateStretchRect("Shougunu_Runtime_BackVFX", targetRect);
        presenceRoot = CreateStretchRect("Shougunu_Runtime_Presence", backRoot);
        presenceGroup = presenceRoot.gameObject.AddComponent<CanvasGroup>();
        presenceGroup.blocksRaycasts = false;
        presenceGroup.interactable = false;

        motionRoot = CreateStretchRect("Shougunu_Runtime_Motion", targetRect);
        poseRoot = CreateStretchRect("Shougunu_Runtime_Pose", motionRoot);
        frontRoot = CreateStretchRect("Shougunu_Runtime_FrontVFX", targetRect);

        neutralMotionPosition = motionRoot.anchoredPosition;
        neutralMotionScale = motionRoot.localScale;
        neutralMotionRotation = motionRoot.localRotation;

        bodyImage = CreateImage(
            "Shougunu_Runtime_Body",
            poseRoot,
            poses[PoseKind.Idle].Sprite,
            Color.white,
            false,
            false);
        Stretch(bodyImage.rectTransform);

        bodyFlashImage = CreateImage(
            "Shougunu_Runtime_BodyFlash",
            poseRoot,
            poses[PoseKind.Idle].Sprite,
            Color.clear,
            false,
            false);
        Stretch(bodyFlashImage.rectTransform);

        BuildPersistentPresence();
    }

    private void BuildPersistentPresence()
    {
        presenceGlowImage = CreateImage(
            "Presence_LowFrequencyGlow",
            presenceRoot,
            softCircleSprite,
            new Color(0.47f, 0.72f, 0.83f, 0.12f),
            true,
            false);
        presenceGlow = presenceGlowImage.rectTransform;
        ConfigureRect(presenceGlow, new Vector2(650f, 650f), new Vector2(260f, -115f));

        presenceOuterRingImage = CreateImage(
            "Presence_OuterBoneSeal",
            presenceRoot,
            ringSprite,
            new Color(0.70f, 0.82f, 0.86f, 0.22f),
            true,
            false);
        presenceOuterRing = presenceOuterRingImage.rectTransform;
        ConfigureRect(presenceOuterRing, new Vector2(720f, 720f), new Vector2(245f, -120f));

        presenceInnerRingImage = CreateImage(
            "Presence_InnerTalismanSeal",
            presenceRoot,
            ringSprite,
            new Color(0.94f, 0.64f, 0.26f, 0.13f),
            true,
            false);
        presenceInnerRing = presenceInnerRingImage.rectTransform;
        ConfigureRect(presenceInnerRing, new Vector2(510f, 510f), new Vector2(245f, -120f));

        uint seed = 0x51A0B31u;
        for (int i = 0; i < 7; i++)
        {
            Image moteImage = CreateImage(
                "Presence_Mote_" + i,
                presenceRoot,
                softCircleSprite,
                new Color(0.72f, 0.82f, 0.84f, 0.09f),
                true,
                false);
            RectTransform moteRect = moteImage.rectTransform;
            float angle = Next01(ref seed) * Mathf.PI * 2f;
            float radius = Mathf.Lerp(230f, 430f, Next01(ref seed));
            Vector2 basePosition = new Vector2(245f, -105f) +
                                   new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.75f) * radius;
            float size = Mathf.Lerp(18f, 42f, Next01(ref seed));
            ConfigureRect(moteRect, new Vector2(size, size), basePosition);
            ambientMotes.Add(new AmbientMote
            {
                Rect = moteRect,
                Image = moteImage,
                BasePosition = basePosition,
                Phase = Next01(ref seed) * 6.28f,
                Radius = Mathf.Lerp(4f, 12f, Next01(ref seed))
            });
        }
    }

    private void BuildControls()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            return;
        }

        controlPanel = new GameObject(
            "ShougunuPhase1_DevOnlyControls",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        controlPanel.hideFlags = HideFlags.DontSave;
        RectTransform panelRect = controlPanel.GetComponent<RectTransform>();
        panelRect.SetParent(canvas.transform, false);
        panelRect.anchorMin = new Vector2(0.5f, 1f);
        panelRect.anchorMax = new Vector2(0.5f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0f, -8f);
        panelRect.sizeDelta = new Vector2(1040f, 82f);
        Image panelImage = controlPanel.GetComponent<Image>();
        panelImage.sprite = solidSprite;
        panelImage.color = new Color(0.025f, 0.035f, 0.045f, 0.82f);
        panelImage.raycastTarget = true;

        statusText = CreateText(
            "Status",
            panelRect,
            "Shougunu Phase 1 — devOnly visual prototype",
            18,
            TextAnchor.MiddleLeft);
        RectTransform statusRect = statusText.rectTransform;
        statusRect.anchorMin = new Vector2(0f, 0.5f);
        statusRect.anchorMax = new Vector2(0f, 0.5f);
        statusRect.pivot = new Vector2(0f, 0.5f);
        statusRect.anchoredPosition = new Vector2(14f, 20f);
        statusRect.sizeDelta = new Vector2(1010f, 28f);

        string[] labels = { "1 Idle", "2 Attack", "3 Hit", "4 Skill1", "5 Skill2", "6 Skill3", "Space Auto", "F9 ×20" };
        Action[] callbacks =
        {
            () => StartManual(ActionKind.Idle),
            () => StartManual(ActionKind.Attack),
            () => StartManual(ActionKind.Hit),
            () => StartManual(ActionKind.Skill1),
            () => StartManual(ActionKind.Skill2),
            () => StartManual(ActionKind.Skill3),
            ToggleAutoShowcase,
            StartStabilityStressProbe
        };

        const float gap = 6f;
        const float width = 121f;
        float left = 14f;
        for (int i = 0; i < labels.Length; i++)
        {
            CreateButton(panelRect, labels[i], new Vector2(left + i * (width + gap), -20f), new Vector2(width, 30f), callbacks[i]);
        }

    }

    private Text CreateText(string objectName, RectTransform parent, string text, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.hideFlags = HideFlags.DontSave;
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Text label = textObject.GetComponent<Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = alignment;
        label.color = new Color(0.91f, 0.94f, 0.96f, 1f);
        label.raycastTarget = false;
        label.font = ResolveRuntimeFont();
        return label;
    }

    private static Font ResolveRuntimeFont()
    {
        try
        {
            Font legacy = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (legacy != null)
            {
                return legacy;
            }
        }
        catch (ArgumentException)
        {
            // Older Unity/Tuanjie builds expose Arial.ttf instead.
        }

        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private void CreateButton(
        RectTransform parent,
        string label,
        Vector2 position,
        Vector2 size,
        Action callback)
    {
        GameObject buttonObject = new GameObject(
            label,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Button));
        buttonObject.hideFlags = HideFlags.DontSave;
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        Image image = buttonObject.GetComponent<Image>();
        image.sprite = solidSprite;
        image.color = new Color(0.18f, 0.25f, 0.29f, 0.96f);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() => callback());

        Text text = CreateText("Label", rect, label, 14, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
    }

    private void StartManual(ActionKind action)
    {
        if (defeatedPresentationActive && action != ActionKind.Idle)
        {
            return;
        }

        StopPlaybackAndReset();
        if (action == ActionKind.Idle)
        {
            SetStatus("Idle / Presence — authoritative neutral pose");
            return;
        }

        StartCoroutine(ManualActionRoutine(action, ++actionSerial));
    }

    private IEnumerator ManualActionRoutine(ActionKind action, int serial)
    {
        yield return ExecuteAction(action, serial);
        if (serial == actionSerial)
        {
            ResetToAuthoritativeNeutral("Idle / Presence — action complete");
        }
    }

    private void ToggleAutoShowcase()
    {
        if (defeatedPresentationActive)
        {
            return;
        }

        if (autoActive)
        {
            StopPlaybackAndReset();
            SetStatus("Auto stopped — authoritative neutral restored");
            return;
        }

        StopPlaybackAndReset();
        autoActive = true;
        StartCoroutine(AutoShowcaseRoutine(++actionSerial));
    }

    private IEnumerator AutoShowcaseRoutine(int serial)
    {
        SetStatus("Auto Showcase — Space stops and cleans immediately");
        while (serial == actionSerial && autoActive)
        {
            actionActive = false;
            yield return WaitVisualSeconds(1.1f);

            ActionKind[] sequence =
            {
                ActionKind.Attack,
                ActionKind.Hit,
                ActionKind.Skill1,
                ActionKind.Skill2,
                ActionKind.Skill3
            };

            for (int i = 0; i < sequence.Length; i++)
            {
                if (serial != actionSerial || !autoActive)
                {
                    yield break;
                }

                yield return ExecuteAction(sequence[i], serial);
                ResetVisualOnly();
                yield return WaitVisualSeconds(0.38f);
            }

            SetStatus("Auto Showcase — full pass complete, looping");
            autoPassCount++;
            yield return WaitVisualSeconds(0.7f);
        }
    }

    private void StartStabilityStressProbe()
    {
        if (defeatedPresentationActive)
        {
            return;
        }

        StopPlaybackAndReset();
        stressActive = true;
        lastStressPassed = false;
        StartCoroutine(StabilityStressRoutine(++actionSerial));
    }

    private IEnumerator StabilityStressRoutine(int serial)
    {
        const int cycles = 20;
        float previousSpeed = playbackSpeed;
        playbackSpeed = 8f;
        bool passed = true;
        string failure = string.Empty;
        ActionKind[] sequence =
        {
            ActionKind.Attack,
            ActionKind.Hit,
            ActionKind.Skill1,
            ActionKind.Skill2,
            ActionKind.Skill3
        };

        for (int i = 0; i < cycles; i++)
        {
            if (serial != actionSerial)
            {
                playbackSpeed = previousSpeed;
                yield break;
            }

            SetStatus($"F9 stability probe — {i + 1}/{cycles} {sequence[i % sequence.Length]}");
            yield return ExecuteAction(sequence[i % sequence.Length], serial);
            ResetVisualOnly();
            yield return null;

            if (!ValidateNeutralState(out failure))
            {
                passed = false;
                break;
            }
        }

        playbackSpeed = previousSpeed;
        stressActive = false;
        if (passed)
        {
            lastStressPassed = true;
            Debug.Log($"[{PackageId}] STABILITY_STRESS_PASS cycles={cycles}; neutral pose exact; transient registry empty.");
            ResetToAuthoritativeNeutral(
                lastStressPassed
                    ? "F9 ×20 PASS — no drift, no registered residual VFX"
                    : "F9 ×20 result unavailable");
        }
        else
        {
            lastStressPassed = false;
            Debug.LogError($"[{PackageId}] STABILITY_STRESS_FAIL: {failure}");
            ResetToAuthoritativeNeutral("F9 ×20 FAIL — see Console");
        }
    }

    private IEnumerator ExecuteAction(ActionKind action, int serial)
    {
        actionActive = true;
        switch (action)
        {
            case ActionKind.Attack:
                yield return BasicAttackRoutine(serial);
                break;
            case ActionKind.Hit:
                yield return HitFeedbackRoutine(serial);
                break;
            case ActionKind.Skill1:
                yield return SkillOneRoutine(serial);
                break;
            case ActionKind.Skill2:
                yield return SkillTwoRoutine(serial);
                break;
            case ActionKind.Skill3:
                yield return SkillThreeRoutine(serial);
                break;
        }

        actionActive = false;
    }

    private void StartShellBreakPresentation()
    {
        StopPlaybackAndReset();
        StartCoroutine(ShellBreakPresentationRoutine(++actionSerial));
    }

    private IEnumerator ShellBreakPresentationRoutine(int serial)
    {
        actionActive = true;
        SetStatus(
            "Shell Break — distinct layered-shell rupture presentation");
        ApplyPose(PoseKind.Idle);
        presenceGroup.alpha = 0.18f;

        SpawnImpactBurst(
            new Vector2(235f, -105f),
            new Color(0.54f, 0.90f, 1f, 0.65f),
            new Color(0.69f, 0.83f, 0.85f, 0.23f),
            14,
            14,
            0x5B311u);

        yield return TweenVisual(0.16f, t =>
        {
            float burst = EaseOutExpo(t);
            float shake = Mathf.Sin(t * Mathf.PI * 7f) * (1f - t);
            SampleMotion(
                new Vector2(shake * 11f, Mathf.Lerp(0f, -13f, burst)),
                Vector2.Lerp(
                    Vector2.one,
                    new Vector2(1.045f, 0.965f),
                    burst),
                shake * 0.65f);
            presenceOuterRing.localScale =
                Vector3.one * Mathf.Lerp(0.72f, 1.28f, burst);
            presenceInnerRing.localScale =
                Vector3.one * Mathf.Lerp(1.16f, 0.78f, burst);
        });

        yield return TweenVisual(0.36f, t =>
        {
            float settle = EaseInOut(t);
            float shake = Mathf.Sin(t * Mathf.PI * 5f) * (1f - t);
            SampleMotion(
                new Vector2(shake * 6f, Mathf.Lerp(-13f, 0f, settle)),
                Vector2.Lerp(
                    new Vector2(1.045f, 0.965f),
                    Vector2.one,
                    settle),
                shake * 0.35f);
            presenceOuterRing.localScale =
                Vector3.Lerp(Vector3.one * 1.28f, Vector3.one, settle);
            presenceInnerRing.localScale =
                Vector3.Lerp(Vector3.one * 0.78f, Vector3.one, settle);
        });

        if (serial == actionSerial)
        {
            presenceOuterRing.localScale = Vector3.one;
            presenceInnerRing.localScale = Vector3.one;
            ResetToAuthoritativeNeutral(
                "Idle / Shell Break presentation complete");
        }
    }

    private void StartDefeatedPresentation()
    {
        StopPlaybackAndReset();
        defeatedPresentationActive = true;
        actionActive = true;
        StartCoroutine(DefeatedPresentationRoutine(++actionSerial));
    }

    private IEnumerator DefeatedPresentationRoutine(int serial)
    {
        SetStatus(
            "Defeated — terminal presentation; Idle/Reset is required");
        ApplyPose(PoseKind.Idle);
        presenceGroup.alpha = 0.32f;
        yield return TweenVisual(0.55f, t =>
        {
            float settle = EaseInOut(t);
            float shake = Mathf.Sin(t * Mathf.PI * 5f) * (1f - t);
            SampleMotion(
                new Vector2(
                    shake * 5f,
                    Mathf.Lerp(0f, -58f, settle)),
                Vector2.Lerp(
                    Vector2.one,
                    new Vector2(0.92f, 0.86f),
                    settle),
                Mathf.Lerp(0f, 4.5f, settle) + shake * 0.25f);
            presenceGroup.alpha = Mathf.Lerp(0.32f, 0.1f, settle);
        });

        if (serial == actionSerial && defeatedPresentationActive)
        {
            SampleMotion(
                new Vector2(0f, -58f),
                new Vector2(0.92f, 0.86f),
                4.5f);
            presenceGroup.alpha = 0.1f;
            actionActive = true;
        }
    }

    private IEnumerator BasicAttackRoutine(int serial)
    {
        SetStatus("Basic Attack — compress → charge → shield/body impact → hit-stop");
        ApplyPose(PoseKind.Attack);
        presenceGroup.alpha = 0.36f;
        SpawnChargeSeal(new Vector2(-10f, -100f), new Color(0.82f, 0.53f, 0.20f, 0.45f), 0.55f);

        yield return TweenVisual(0.18f, t =>
        {
            float e = EaseInOut(t);
            SampleMotion(
                Vector2.Lerp(Vector2.zero, new Vector2(22f, -30f), e),
                Vector2.Lerp(Vector2.one, new Vector2(1.035f, 0.93f), e),
                Mathf.Lerp(0f, 1.2f, e));
        });

        SpawnDustPuffs(new Vector2(80f, -545f), 5, new Color(0.55f, 0.49f, 0.41f, 0.28f), 0xA771u);
        yield return TweenVisual(0.31f, t =>
        {
            float pulse = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
            SampleMotion(
                Vector2.Lerp(new Vector2(22f, -30f), new Vector2(52f, -36f), t) + new Vector2(pulse * 3f, 0f),
                Vector2.Lerp(new Vector2(1.035f, 0.93f), new Vector2(1.055f, 0.915f), t),
                Mathf.Lerp(1.2f, 1.8f, t));
            bodyFlashImage.color = new Color(1f, 0.64f, 0.25f, 0.07f + t * 0.16f);
        });

        SpawnAfterImage(new Color(0.73f, 0.35f, 0.12f, 0.24f), new Vector2(36f, 0f), 0.18f);
        yield return TweenVisual(0.095f, t =>
        {
            float e = EaseOutExpo(t);
            SampleMotion(
                Vector2.Lerp(new Vector2(52f, -36f), new Vector2(-150f, 10f), e),
                Vector2.Lerp(new Vector2(1.055f, 0.915f), new Vector2(1.075f, 0.985f), e),
                Mathf.Lerp(1.8f, -2.7f, e));
            bodyFlashImage.color = new Color(1f, 0.9f, 0.66f, Mathf.Lerp(0.23f, 0.06f, e));
        });

        SpawnImpactBurst(
            new Vector2(-525f, -165f),
            new Color(1f, 0.67f, 0.22f, 0.9f),
            new Color(0.53f, 0.46f, 0.38f, 0.4f),
            12,
            10,
            0xB45A11u);
        yield return WaitVisualSeconds(0.075f);

        yield return TweenVisual(0.21f, t =>
        {
            float damp = Mathf.Sin(t * Mathf.PI * 3f) * (1f - t);
            SampleMotion(
                Vector2.Lerp(new Vector2(-150f, 10f), new Vector2(-82f, -7f), EaseOutCubic(t)) + new Vector2(damp * 14f, 0f),
                Vector2.Lerp(new Vector2(1.075f, 0.985f), new Vector2(1.01f, 1.005f), t),
                Mathf.Lerp(-2.7f, -0.7f, t) + damp * 0.6f);
        });

        yield return TweenVisual(0.32f, t =>
        {
            float e = EaseInOut(t);
            SampleMotion(
                Vector2.Lerp(new Vector2(-82f, -7f), Vector2.zero, e),
                Vector2.Lerp(new Vector2(1.01f, 1.005f), Vector2.one, e),
                Mathf.Lerp(-0.7f, 0f, e));
            bodyFlashImage.color = Color.Lerp(new Color(1f, 0.78f, 0.44f, 0.05f), Color.clear, e);
        });
    }

    private IEnumerator HitFeedbackRoutine(int serial)
    {
        SetStatus("Hit Feedback — short displacement, warm core flash, debris, exact recovery");
        ApplyPose(PoseKind.Hit);
        presenceGroup.alpha = 0.28f;
        bodyFlashImage.color = new Color(1f, 0.92f, 0.77f, 0.72f);
        SpawnImpactBurst(
            new Vector2(285f, -60f),
            new Color(1f, 0.82f, 0.53f, 0.72f),
            new Color(0.62f, 0.47f, 0.34f, 0.3f),
            7,
            7,
            0x11717u);

        yield return TweenVisual(0.07f, t =>
        {
            float e = EaseOutExpo(t);
            SampleMotion(
                Vector2.Lerp(Vector2.zero, new Vector2(54f, -9f), e),
                Vector2.Lerp(Vector2.one, new Vector2(0.985f, 1.015f), e),
                Mathf.Lerp(0f, -3.4f, e));
            bodyFlashImage.color = Color.Lerp(
                new Color(1f, 0.95f, 0.82f, 0.76f),
                new Color(1f, 0.51f, 0.25f, 0.28f),
                e);
        });

        yield return WaitVisualSeconds(0.07f);
        SpawnAfterImage(new Color(0.82f, 0.28f, 0.13f, 0.18f), new Vector2(-18f, 0f), 0.16f);

        yield return TweenVisual(0.29f, t =>
        {
            float decay = 1f - t;
            float shake = Mathf.Sin(t * Mathf.PI * 5f) * decay;
            SampleMotion(
                Vector2.Lerp(new Vector2(54f, -9f), Vector2.zero, EaseOutCubic(t)) + new Vector2(shake * 8f, 0f),
                Vector2.Lerp(new Vector2(0.985f, 1.015f), Vector2.one, t),
                Mathf.Lerp(-3.4f, 0f, EaseOutCubic(t)) + shake * 0.45f);
            bodyFlashImage.color = Color.Lerp(new Color(1f, 0.48f, 0.22f, 0.28f), Color.clear, t);
        });
    }

    private IEnumerator SkillOneRoutine(int serial)
    {
        SetStatus("Skill 1 — gate ward / layered barrier presentation");
        ApplyPose(PoseKind.Skill1);
        presenceGroup.alpha = 0.18f;
        bodyImage.color = new Color(0.88f, 0.94f, 0.96f, 1f);

        SpawnBarrierWard();
        yield return TweenVisual(0.36f, t =>
        {
            float e = EaseOutCubic(t);
            SampleMotion(
                Vector2.Lerp(new Vector2(0f, -20f), new Vector2(0f, 5f), e),
                Vector2.Lerp(new Vector2(0.92f, 0.92f), new Vector2(1.025f, 1.025f), e),
                0f);
            bodyFlashImage.color = new Color(0.67f, 0.93f, 1f, Mathf.Sin(t * Mathf.PI) * 0.24f);
        });

        SpawnConvergingWardShards(new Vector2(240f, -105f), 12, 0x51111u);
        yield return TweenVisual(0.42f, t =>
        {
            float pulse = Mathf.Sin(t * Mathf.PI * 2f);
            SampleMotion(
                new Vector2(0f, 5f + pulse * 3f),
                new Vector2(1.025f + pulse * 0.008f, 1.025f + pulse * 0.012f),
                pulse * 0.12f);
            bodyFlashImage.color = new Color(0.78f, 0.96f, 1f, 0.10f + Mathf.Max(0f, pulse) * 0.13f);
        });

        SpawnImpactBurst(
            new Vector2(240f, -105f),
            new Color(0.54f, 0.90f, 1f, 0.65f),
            new Color(0.69f, 0.83f, 0.85f, 0.23f),
            8,
            5,
            0x51112u);
        yield return WaitVisualSeconds(0.17f);

        yield return TweenVisual(0.38f, t =>
        {
            float e = EaseInOut(t);
            SampleMotion(
                Vector2.Lerp(new Vector2(0f, 5f), Vector2.zero, e),
                Vector2.Lerp(new Vector2(1.025f, 1.025f), Vector2.one, e),
                0f);
            bodyFlashImage.color = Color.Lerp(new Color(0.71f, 0.91f, 1f, 0.14f), Color.clear, e);
        });
    }

    private IEnumerator SkillTwoRoutine(int serial)
    {
        SetStatus("Skill 2 — rope/talisman binding burst; source streaks treated as motion smear");
        ApplyPose(PoseKind.Skill2);
        presenceGroup.alpha = 0.08f;
        bodyImage.color = new Color(0.73f, 0.64f, 0.57f, 0.84f);

        SpawnBindingField();
        yield return TweenVisual(0.16f, t =>
        {
            float e = EaseOutExpo(t);
            SampleMotion(
                Vector2.Lerp(new Vector2(82f, -8f), new Vector2(4f, 0f), e),
                Vector2.Lerp(new Vector2(1.055f, 0.98f), Vector2.one, e),
                Mathf.Lerp(1.4f, 0f, e));
            bodyFlashImage.color = new Color(1f, 0.66f, 0.27f, Mathf.Sin(t * Mathf.PI) * 0.2f);
        });

        SpawnAfterImage(new Color(0.42f, 0.16f, 0.09f, 0.17f), new Vector2(42f, 0f), 0.22f);
        yield return TweenVisual(0.36f, t =>
        {
            float snap = Mathf.Sin(t * Mathf.PI * 5f) * (1f - t);
            SampleMotion(new Vector2(snap * 7f, 0f), Vector2.one, snap * 0.35f);
            bodyFlashImage.color = new Color(1f, 0.47f, 0.18f, 0.06f + Mathf.Abs(snap) * 0.12f);
        });

        SpawnImpactBurst(
            new Vector2(190f, -80f),
            new Color(1f, 0.45f, 0.14f, 0.62f),
            new Color(0.40f, 0.25f, 0.16f, 0.22f),
            6,
            8,
            0x52222u);
        yield return WaitVisualSeconds(0.16f);

        // The authored image is intentionally a short cut. The procedural rope language
        // remains while the clean idle sheet returns for recovery.
        ApplyPose(PoseKind.Idle);
        yield return TweenVisual(0.34f, t =>
        {
            float e = EaseInOut(t);
            SampleMotion(Vector2.Lerp(new Vector2(20f, 0f), Vector2.zero, e), Vector2.one, 0f);
        });
    }

    private IEnumerator SkillThreeRoutine(int serial)
    {
        SetStatus("Skill 3 — ground seal / bone-fire eruption; authored backdrop kept as a short beat");
        ApplyPose(PoseKind.Skill3);
        presenceGroup.alpha = 0.04f;
        bodyImage.color = new Color(0.79f, 0.47f, 0.34f, 0.82f);

        SpawnGroundSealAndBoneFire();
        yield return TweenVisual(0.22f, t =>
        {
            float e = EaseOutCubic(t);
            SampleMotion(
                Vector2.Lerp(new Vector2(0f, -42f), new Vector2(0f, 8f), e),
                Vector2.Lerp(new Vector2(0.96f, 0.90f), new Vector2(1.02f, 1.035f), e),
                0f);
            bodyFlashImage.color = new Color(1f, 0.37f, 0.13f, Mathf.Sin(t * Mathf.PI) * 0.24f);
        });

        SpawnVerticalFireBurst(new Vector2(-45f, -485f), 13, 0x53331u);
        yield return WaitVisualSeconds(0.13f);
        SpawnImpactBurst(
            new Vector2(-35f, -465f),
            new Color(1f, 0.32f, 0.08f, 0.75f),
            new Color(0.28f, 0.20f, 0.18f, 0.38f),
            11,
            9,
            0x53332u);

        yield return TweenVisual(0.34f, t =>
        {
            float quake = Mathf.Sin(t * Mathf.PI * 7f) * (1f - t);
            SampleMotion(
                new Vector2(quake * 7f, Mathf.Lerp(8f, 0f, t)),
                Vector2.Lerp(new Vector2(1.02f, 1.035f), Vector2.one, t),
                quake * 0.32f);
            bodyFlashImage.color = new Color(1f, 0.31f, 0.09f, 0.08f + Mathf.Abs(quake) * 0.11f);
        });

        ApplyPose(PoseKind.Idle);
        yield return TweenVisual(0.38f, t =>
        {
            float e = EaseInOut(t);
            SampleMotion(Vector2.Lerp(new Vector2(0f, 12f), Vector2.zero, e), Vector2.one, 0f);
        });
    }

    private void SpawnChargeSeal(Vector2 position, Color color, float lifetime)
    {
        Image ring = CreateTransientImage(
            "Attack_ChargeSeal",
            backRoot,
            ringSprite,
            color,
            true,
            new Vector2(380f, 380f),
            position);
        StartCoroutine(AnimateImage(
            ring,
            position,
            position,
            new Vector2(0.65f, 0.65f),
            new Vector2(1.22f, 1.22f),
            color.a,
            0f,
            85f,
            lifetime));
    }

    private void SpawnBarrierWard()
    {
        Color cyan = new Color(0.49f, 0.88f, 1f, 0.45f);
        Image outer = CreateTransientImage(
            "Skill1_Back_OuterWard",
            backRoot,
            ringSprite,
            cyan,
            true,
            new Vector2(980f, 980f),
            new Vector2(190f, -120f));
        StartCoroutine(AnimateImage(
            outer,
            outer.rectTransform.anchoredPosition,
            outer.rectTransform.anchoredPosition,
            new Vector2(0.72f, 0.72f),
            new Vector2(1.08f, 1.08f),
            0f,
            0.36f,
            72f,
            1.25f));

        Image inner = CreateTransientImage(
            "Skill1_Back_InnerWard",
            backRoot,
            ringSprite,
            new Color(0.95f, 0.72f, 0.30f, 0.34f),
            true,
            new Vector2(700f, 700f),
            new Vector2(190f, -120f));
        StartCoroutine(AnimateImage(
            inner,
            inner.rectTransform.anchoredPosition,
            inner.rectTransform.anchoredPosition,
            new Vector2(1.15f, 1.15f),
            new Vector2(0.86f, 0.86f),
            0f,
            0.29f,
            -105f,
            1.15f));

        for (int i = -1; i <= 1; i++)
        {
            Vector2 position = new Vector2(190f + i * 205f, -90f);
            Image panel = CreateTransientImage(
                "Skill1_Front_BarrierPanel_" + i,
                frontRoot,
                solidSprite,
                new Color(0.45f, 0.86f, 0.96f, i == 0 ? 0.10f : 0.065f),
                false,
                new Vector2(165f, 700f),
                position);
            StartCoroutine(AnimateImage(
                panel,
                position + new Vector2(0f, -80f),
                position,
                new Vector2(1f, 0.15f),
                Vector2.one,
                0f,
                panel.color.a,
                0f,
                0.48f));

            Image edge = CreateTransientImage(
                "Skill1_Front_BarrierEdge_" + i,
                frontRoot,
                solidSprite,
                new Color(0.64f, 0.94f, 1f, 0.52f),
                true,
                new Vector2(8f, 720f),
                position);
            StartCoroutine(AnimateImage(
                edge,
                position + new Vector2(0f, -80f),
                position,
                new Vector2(1f, 0.1f),
                Vector2.one,
                0f,
                0.42f,
                0f,
                0.5f));
        }
    }

    private void SpawnConvergingWardShards(Vector2 centre, int count, uint seed)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Next01(ref seed) * Mathf.PI * 2f;
            float radius = Mathf.Lerp(380f, 620f, Next01(ref seed));
            Vector2 start = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            Vector2 end = centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 45f;
            Image shard = CreateTransientImage(
                "Skill1_Front_ConvergingShard_" + i,
                frontRoot,
                solidSprite,
                new Color(0.69f, 0.94f, 1f, 0.55f),
                true,
                new Vector2(Mathf.Lerp(4f, 9f, Next01(ref seed)), Mathf.Lerp(22f, 48f, Next01(ref seed))),
                start);
            shard.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg - 90f);
            StartCoroutine(AnimateImage(
                shard,
                start,
                end,
                new Vector2(0.65f, 0.65f),
                Vector2.one,
                0f,
                shard.color.a,
                15f,
                0.43f));
        }
    }

    private void SpawnBindingField()
    {
        uint seed = 0xB1D1A6u;
        Vector2 bindingCentre = new Vector2(170f, -65f);
        for (int i = 0; i < 8; i++)
        {
            Vector2 start = new Vector2(
                Mathf.Lerp(-820f, -650f, Next01(ref seed)),
                Mathf.Lerp(-420f, 370f, Next01(ref seed)));
            Vector2 end = bindingCentre + new Vector2(
                Mathf.Lerp(-90f, 90f, Next01(ref seed)),
                Mathf.Lerp(-180f, 180f, Next01(ref seed)));
            Color ropeColor = i % 2 == 0
                ? new Color(0.54f, 0.18f, 0.10f, 0.78f)
                : new Color(0.84f, 0.52f, 0.18f, 0.70f);
            Image line = CreateTransientImage(
                "Skill2_Front_BindingRope_" + i,
                frontRoot,
                solidSprite,
                ropeColor,
                false,
                new Vector2(1f, Mathf.Lerp(7f, 14f, Next01(ref seed))),
                start);
            StartCoroutine(AnimateLineExtension(line, start, end, 0.18f + i * 0.025f));

            if (i % 2 == 0)
            {
                Vector2 talismanStart = Vector2.Lerp(start, end, 0.28f);
                Vector2 talismanEnd = Vector2.Lerp(start, end, 0.82f);
                Image talisman = CreateTransientImage(
                    "Skill2_Front_TalismanSlip_" + i,
                    frontRoot,
                    solidSprite,
                    new Color(0.93f, 0.78f, 0.48f, 0.85f),
                    false,
                    new Vector2(34f, 78f),
                    talismanStart);
                talisman.rectTransform.localRotation = line.rectTransform.localRotation * Quaternion.Euler(0f, 0f, 90f);
                StartCoroutine(AnimateImage(
                    talisman,
                    talismanStart,
                    talismanEnd,
                    new Vector2(0.7f, 0.7f),
                    Vector2.one,
                    0f,
                    0.8f,
                    38f,
                    0.38f));
            }
        }

        Image knot = CreateTransientImage(
            "Skill2_Back_BindingKnot",
            backRoot,
            ringSprite,
            new Color(1f, 0.35f, 0.12f, 0.48f),
            true,
            new Vector2(530f, 530f),
            bindingCentre);
        StartCoroutine(AnimateImage(
            knot,
            bindingCentre,
            bindingCentre,
            new Vector2(1.25f, 1.25f),
            new Vector2(0.58f, 0.58f),
            0f,
            0.42f,
            -140f,
            0.58f));

        for (int i = 0; i < 7; i++)
        {
            float y = -360f + i * 115f;
            Image speedLine = CreateTransientImage(
                "Skill2_Front_SpeedLine_" + i,
                frontRoot,
                solidSprite,
                new Color(1f, 0.55f, 0.18f, 0.24f),
                true,
                new Vector2(Mathf.Lerp(180f, 360f, Next01(ref seed)), 4f),
                new Vector2(-720f, y));
            StartCoroutine(AnimateImage(
                speedLine,
                new Vector2(-720f, y),
                new Vector2(80f, y + Mathf.Lerp(-40f, 40f, Next01(ref seed))),
                new Vector2(0.15f, 1f),
                Vector2.one,
                0f,
                0.28f,
                0f,
                0.28f));
        }
    }

    private void SpawnGroundSealAndBoneFire()
    {
        Vector2 ground = new Vector2(-30f, -505f);
        Image outer = CreateTransientImage(
            "Skill3_Back_GroundSealOuter",
            backRoot,
            ringSprite,
            new Color(1f, 0.25f, 0.06f, 0.55f),
            true,
            new Vector2(1250f, 380f),
            ground);
        StartCoroutine(AnimateImage(
            outer,
            ground,
            ground,
            new Vector2(0.35f, 0.35f),
            Vector2.one,
            0f,
            0.48f,
            55f,
            0.62f));

        Image inner = CreateTransientImage(
            "Skill3_Back_GroundSealInner",
            backRoot,
            ringSprite,
            new Color(1f, 0.67f, 0.15f, 0.46f),
            true,
            new Vector2(820f, 245f),
            ground);
        StartCoroutine(AnimateImage(
            inner,
            ground,
            ground,
            new Vector2(1.1f, 1.1f),
            new Vector2(0.82f, 0.82f),
            0f,
            0.4f,
            -95f,
            0.68f));

        for (int i = 0; i < 14; i++)
        {
            float angle = i / 14f * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.3f);
            Image spoke = CreateTransientImage(
                "Skill3_Back_SealSpoke_" + i,
                backRoot,
                solidSprite,
                new Color(0.95f, 0.26f, 0.08f, 0.24f),
                true,
                new Vector2(240f, 5f),
                ground + direction * 310f);
            spoke.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            StartCoroutine(AnimateImage(
                spoke,
                ground,
                ground + direction * 310f,
                new Vector2(0.1f, 1f),
                Vector2.one,
                0f,
                0.22f,
                0f,
                0.34f));
        }
    }

    private void SpawnVerticalFireBurst(Vector2 centre, int count, uint seed)
    {
        for (int i = 0; i < count; i++)
        {
            float x = Mathf.Lerp(-570f, 570f, count <= 1 ? 0.5f : i / (float)(count - 1));
            x += Mathf.Lerp(-32f, 32f, Next01(ref seed));
            float height = Mathf.Lerp(170f, 360f, Next01(ref seed));
            Vector2 start = centre + new Vector2(x, Mathf.Lerp(-15f, 25f, Next01(ref seed)));
            Vector2 end = start + new Vector2(Mathf.Lerp(-25f, 25f, Next01(ref seed)), height * 0.42f);
            Color color = i % 3 == 0
                ? new Color(1f, 0.72f, 0.18f, 0.72f)
                : new Color(1f, 0.20f, 0.055f, 0.62f);
            Image flame = CreateTransientImage(
                "Skill3_Front_BoneFire_" + i,
                frontRoot,
                flameSprite,
                color,
                true,
                new Vector2(Mathf.Lerp(70f, 130f, Next01(ref seed)), height),
                start);
            StartCoroutine(AnimateImage(
                flame,
                start,
                end,
                new Vector2(0.45f, 0.18f),
                new Vector2(1.15f, 1f),
                0f,
                color.a,
                Mathf.Lerp(-18f, 18f, Next01(ref seed)),
                Mathf.Lerp(0.45f, 0.72f, Next01(ref seed))));
        }

        SpawnDustPuffs(centre, 9, new Color(0.24f, 0.19f, 0.18f, 0.42f), seed ^ 0xA91u);
    }

    private void SpawnImpactBurst(
        Vector2 position,
        Color glowColor,
        Color dustColor,
        int rayCount,
        int shardCount,
        uint seed)
    {
        Image glow = CreateTransientImage(
            "Impact_Front_CoreGlow",
            frontRoot,
            softCircleSprite,
            glowColor,
            true,
            new Vector2(330f, 330f),
            position);
        StartCoroutine(AnimateImage(
            glow,
            position,
            position,
            new Vector2(0.18f, 0.18f),
            new Vector2(1.25f, 1.25f),
            glowColor.a,
            0f,
            0f,
            0.25f));

        Image ring = CreateTransientImage(
            "Impact_Front_DustRing",
            frontRoot,
            ringSprite,
            new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * 0.72f),
            true,
            new Vector2(250f, 250f),
            position);
        StartCoroutine(AnimateImage(
            ring,
            position,
            position,
            new Vector2(0.35f, 0.35f),
            new Vector2(2.25f, 2.25f),
            ring.color.a,
            0f,
            35f,
            0.35f));

        for (int i = 0; i < rayCount; i++)
        {
            float angle = (i / (float)rayCount + Next01(ref seed) * 0.08f) * Mathf.PI * 2f;
            float length = Mathf.Lerp(90f, 235f, Next01(ref seed));
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 end = position + direction * Mathf.Lerp(170f, 310f, Next01(ref seed));
            Image ray = CreateTransientImage(
                "Impact_Front_Ray_" + i,
                frontRoot,
                solidSprite,
                new Color(glowColor.r, glowColor.g, glowColor.b, glowColor.a * 0.62f),
                true,
                new Vector2(length, Mathf.Lerp(3f, 9f, Next01(ref seed))),
                position);
            ray.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle * Mathf.Rad2Deg);
            StartCoroutine(AnimateImage(
                ray,
                position,
                end,
                new Vector2(0.1f, 1f),
                Vector2.one,
                ray.color.a,
                0f,
                0f,
                0.23f));
        }

        for (int i = 0; i < shardCount; i++)
        {
            float angle = Mathf.Lerp(-0.3f, 1.3f, Next01(ref seed)) * Mathf.PI;
            float distance = Mathf.Lerp(150f, 390f, Next01(ref seed));
            Vector2 end = position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance + Vector2.down * 55f;
            Color shardColor = Color.Lerp(
                new Color(0.88f, 0.83f, 0.72f, 0.85f),
                new Color(0.42f, 0.31f, 0.23f, 0.72f),
                Next01(ref seed));
            Image shard = CreateTransientImage(
                "Impact_Front_AlphaShard_" + i,
                frontRoot,
                solidSprite,
                shardColor,
                false,
                new Vector2(Mathf.Lerp(8f, 18f, Next01(ref seed)), Mathf.Lerp(18f, 52f, Next01(ref seed))),
                position);
            StartCoroutine(AnimateImage(
                shard,
                position,
                end,
                Vector2.one,
                new Vector2(0.55f, 0.55f),
                shardColor.a,
                0f,
                Mathf.Lerp(-180f, 180f, Next01(ref seed)),
                Mathf.Lerp(0.32f, 0.55f, Next01(ref seed))));
        }

        SpawnDustPuffs(position + Vector2.down * 35f, 6, dustColor, seed ^ 0xD057u);
    }

    private void SpawnDustPuffs(Vector2 centre, int count, Color color, uint seed)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 start = centre + new Vector2(
                Mathf.Lerp(-110f, 110f, Next01(ref seed)),
                Mathf.Lerp(-20f, 30f, Next01(ref seed)));
            Vector2 end = start + new Vector2(
                Mathf.Lerp(-170f, 170f, Next01(ref seed)),
                Mathf.Lerp(45f, 140f, Next01(ref seed)));
            float size = Mathf.Lerp(75f, 155f, Next01(ref seed));
            Image puff = CreateTransientImage(
                "AlphaDust_" + i,
                frontRoot,
                softCircleSprite,
                color,
                false,
                new Vector2(size, size * Mathf.Lerp(0.55f, 0.9f, Next01(ref seed))),
                start);
            StartCoroutine(AnimateImage(
                puff,
                start,
                end,
                new Vector2(0.35f, 0.35f),
                new Vector2(1.45f, 1.05f),
                color.a,
                0f,
                Mathf.Lerp(-35f, 35f, Next01(ref seed)),
                Mathf.Lerp(0.42f, 0.72f, Next01(ref seed))));
        }
    }

    private void SpawnAfterImage(Color color, Vector2 offset, float lifetime)
    {
        PoseAsset asset = poses[currentPose];
        Image afterImage = CreateTransientImage(
            "Front_AlphaAfterImage",
            frontRoot,
            asset.Sprite,
            color,
            false,
            targetRect.rect.size,
            motionRoot.anchoredPosition + asset.Calibration.Offset + offset);
        afterImage.preserveAspect = false;
        afterImage.rectTransform.localScale = new Vector3(
            motionRoot.localScale.x * asset.Calibration.Scale,
            motionRoot.localScale.y * asset.Calibration.Scale,
            1f);
        afterImage.rectTransform.localRotation = motionRoot.localRotation;
        StartCoroutine(AnimateImage(
            afterImage,
            afterImage.rectTransform.anchoredPosition,
            afterImage.rectTransform.anchoredPosition + offset,
            Vector2.one,
            new Vector2(1.015f, 1.015f),
            color.a,
            0f,
            0f,
            lifetime));
    }

    private IEnumerator AnimateLineExtension(Image line, Vector2 start, Vector2 end, float duration)
    {
        RectTransform rect = line.rectTransform;
        Vector2 delta = end - start;
        float fullLength = delta.magnitude;
        float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
        float width = rect.sizeDelta.y;
        float alpha = line.color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime * playbackSpeed;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = EaseOutExpo(t);
            rect.sizeDelta = new Vector2(Mathf.Max(1f, fullLength * e), width);
            rect.anchoredPosition = start + delta * (e * 0.5f);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle);
            Color color = line.color;
            color.a = alpha * Mathf.SmoothStep(0f, 1f, Mathf.Min(1f, t * 4f));
            line.color = color;
            yield return null;
        }
    }

    private IEnumerator AnimateImage(
        Image image,
        Vector2 startPosition,
        Vector2 endPosition,
        Vector2 startScale,
        Vector2 endScale,
        float startAlpha,
        float endAlpha,
        float rotationDelta,
        float duration)
    {
        if (image == null)
        {
            yield break;
        }

        RectTransform rect = image.rectTransform;
        float startRotation = rect.localEulerAngles.z;
        Color baseColor = image.color;
        float elapsed = 0f;
        while (elapsed < duration && image != null)
        {
            elapsed += Time.unscaledDeltaTime * playbackSpeed;
            float t = Mathf.Clamp01(elapsed / duration);
            float e = EaseOutCubic(t);
            rect.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, e);
            Vector2 scale = Vector2.LerpUnclamped(startScale, endScale, e);
            rect.localScale = new Vector3(scale.x, scale.y, 1f);
            rect.localRotation = Quaternion.Euler(0f, 0f, startRotation + rotationDelta * e);
            Color color = baseColor;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            image.color = color;
            yield return null;
        }
    }

    private IEnumerator TweenVisual(float duration, Action<float> sample)
    {
        float elapsed = 0f;
        sample(0f);
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime * playbackSpeed;
            sample(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        sample(1f);
    }

    private IEnumerator WaitVisualSeconds(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime * playbackSpeed;
            yield return null;
        }
    }

    private void ApplyPose(PoseKind pose)
    {
        currentPose = pose;
        PoseAsset asset = poses[pose];
        poseRoot.anchoredPosition = asset.Calibration.Offset;
        poseRoot.localScale = new Vector3(asset.Calibration.Scale, asset.Calibration.Scale, 1f);
        poseRoot.localRotation = Quaternion.identity;
        bodyImage.sprite = asset.Sprite;
        bodyFlashImage.sprite = asset.Sprite;
        bodyImage.color = Color.white;
        bodyFlashImage.color = Color.clear;
    }

    private void SampleMotion(Vector2 offset, Vector2 scale, float rotation)
    {
        motionRoot.anchoredPosition = neutralMotionPosition + offset;
        motionRoot.localScale = new Vector3(
            neutralMotionScale.x * scale.x,
            neutralMotionScale.y * scale.y,
            neutralMotionScale.z);
        motionRoot.localRotation = neutralMotionRotation * Quaternion.Euler(0f, 0f, rotation);
    }

    private void UpdatePresence(float time)
    {
        if (presenceGroup == null)
        {
            return;
        }

        if (!actionActive && !stressActive)
        {
            float breath = 0.5f + 0.5f * Mathf.Sin(time * 1.28f);
            float settle = Mathf.Sin(time * 0.64f + 0.35f);
            SampleMotion(
                new Vector2(settle * 1.2f, Mathf.Lerp(-5.5f, 4.5f, breath)),
                new Vector2(
                    1f + Mathf.Sin(time * 1.28f + 1.1f) * 0.0035f,
                    0.991f + breath * 0.018f),
                settle * 0.22f);
            presenceGroup.alpha = Mathf.Lerp(presenceGroup.alpha, 1f, Time.unscaledDeltaTime * 3f);
        }

        presenceOuterRing.localRotation = Quaternion.Euler(0f, 0f, time * 4.2f);
        presenceInnerRing.localRotation = Quaternion.Euler(0f, 0f, -time * 6.1f);
        float lowPulse = 0.5f + 0.5f * Mathf.Sin(time * 0.82f);
        presenceOuterRingImage.color = new Color(0.70f, 0.82f, 0.86f, 0.15f + lowPulse * 0.09f);
        presenceInnerRingImage.color = new Color(0.94f, 0.64f, 0.26f, 0.07f + (1f - lowPulse) * 0.08f);
        presenceGlowImage.color = new Color(0.47f, 0.72f, 0.83f, 0.07f + lowPulse * 0.07f);
        float glowScale = 0.94f + lowPulse * 0.10f;
        presenceGlow.localScale = new Vector3(glowScale, glowScale, 1f);

        for (int i = 0; i < ambientMotes.Count; i++)
        {
            AmbientMote mote = ambientMotes[i];
            float phase = time * (0.22f + i * 0.015f) + mote.Phase;
            mote.Rect.anchoredPosition = mote.BasePosition + new Vector2(
                Mathf.Cos(phase) * mote.Radius,
                Mathf.Sin(phase * 1.3f) * mote.Radius);
            Color color = mote.Image.color;
            color.a = 0.035f + (0.5f + 0.5f * Mathf.Sin(phase * 1.7f)) * 0.07f;
            mote.Image.color = color;
        }
    }

    private void StopPlaybackAndReset()
    {
        actionSerial++;
        autoActive = false;
        stressActive = false;
        defeatedPresentationActive = false;
        actionActive = false;
        playbackSpeed = 1f;
        StopAllCoroutines();
        ClearTransientObjects();
        ResetVisualOnly();
    }

    private void ResetToAuthoritativeNeutral(string status)
    {
        ResetVisualOnly();
        SetStatus(status);
    }

    private void ResetVisualOnly()
    {
        ClearTransientObjects();
        ApplyPose(PoseKind.Idle);
        SampleMotion(Vector2.zero, Vector2.one, 0f);
        bodyImage.color = Color.white;
        bodyFlashImage.color = Color.clear;
        presenceGroup.alpha = 1f;
        actionActive = false;
    }

    private bool ValidateNeutralState(out string failure)
    {
        return ValidateNeutralState(out failure, false);
    }

    private bool ValidateNeutralState(
        out string failure,
        bool allowIdlePresenceSample)
    {
        Vector2 positionDelta = motionRoot.anchoredPosition - neutralMotionPosition;
        Vector3 scaleDelta = motionRoot.localScale - neutralMotionScale;
        float rotationDelta = Quaternion.Angle(
            motionRoot.localRotation,
            neutralMotionRotation);

        if (allowIdlePresenceSample)
        {
            if (Mathf.Abs(positionDelta.x) > 1.25f ||
                positionDelta.y < -5.55f ||
                positionDelta.y > 4.55f)
            {
                failure = "idle presence position escaped its absolute neutral envelope";
                return false;
            }

            if (Mathf.Abs(scaleDelta.x) > 0.0036f ||
                scaleDelta.y < -0.0091f ||
                scaleDelta.y > 0.0091f ||
                rotationDelta > 0.225f)
            {
                failure = "idle presence scale/rotation escaped its absolute neutral envelope";
                return false;
            }
        }
        else if (positionDelta.sqrMagnitude > 0.0001f)
        {
            failure = "motion position drift";
            return false;
        }

        if (!allowIdlePresenceSample && scaleDelta.sqrMagnitude > 0.0001f)
        {
            failure = "motion scale drift";
            return false;
        }

        if (!allowIdlePresenceSample && rotationDelta > 0.001f)
        {
            failure = "motion rotation drift";
            return false;
        }

        if (currentPose != PoseKind.Idle)
        {
            failure = "pose did not return to idle";
            return false;
        }

        PoseAsset idleAsset = poses[PoseKind.Idle];
        if ((poseRoot.anchoredPosition - idleAsset.Calibration.Offset).sqrMagnitude >
            0.0001f ||
            Mathf.Abs(poseRoot.localScale.x - idleAsset.Calibration.Scale) >
            0.0001f ||
            Mathf.Abs(poseRoot.localScale.y - idleAsset.Calibration.Scale) >
            0.0001f ||
            Quaternion.Angle(poseRoot.localRotation, Quaternion.identity) >
            0.001f ||
            bodyImage.sprite != idleAsset.Sprite)
        {
            failure = "idle pose calibration or sprite was not authoritative";
            return false;
        }

        if (transientObjects.Count != 0)
        {
            failure = "transient registry was not empty";
            return false;
        }

        failure = string.Empty;
        return true;
    }

    private void ClearTransientObjects()
    {
        for (int i = transientObjects.Count - 1; i >= 0; i--)
        {
            if (transientObjects[i] != null)
            {
                Destroy(transientObjects[i]);
            }
        }

        transientObjects.Clear();
    }

    private Image CreateTransientImage(
        string objectName,
        RectTransform parent,
        Sprite sprite,
        Color color,
        bool additive,
        Vector2 size,
        Vector2 position)
    {
        Image image = CreateImage(objectName, parent, sprite, color, additive, true);
        ConfigureRect(image.rectTransform, size, position);
        return image;
    }

    private Image CreateImage(
        string objectName,
        RectTransform parent,
        Sprite sprite,
        Color color,
        bool additive,
        bool transient)
    {
        GameObject imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.hideFlags = HideFlags.DontSave;
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.preserveAspect = false;
        image.raycastTarget = false;
        if (additive && additiveMaterial != null)
        {
            image.material = additiveMaterial;
        }

        if (transient)
        {
            transientObjects.Add(imageObject);
        }

        return image;
    }

    private static RectTransform CreateStretchRect(string objectName, RectTransform parent)
    {
        GameObject root = new GameObject(objectName, typeof(RectTransform));
        root.hideFlags = HideFlags.DontSave;
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        Stretch(rect);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void ConfigureRect(RectTransform rect, Vector2 size, Vector2 position)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private static float Next01(ref uint state)
    {
        state = state * 1664525u + 1013904223u;
        return (state & 0x00FFFFFFu) / 16777215f;
    }

    private static float EaseOutCubic(float t)
    {
        float inverse = 1f - Mathf.Clamp01(t);
        return 1f - inverse * inverse * inverse;
    }

    private static float EaseInOut(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    private static float EaseOutExpo(float t)
    {
        t = Mathf.Clamp01(t);
        return t >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * t);
    }

#if UNITY_EDITOR
    private const string LeaseQaScenePath =
        "Assets/_Game/Scenes/Scene_TalismanBag_V04_BattleSandboxPreview.unity";
    private const string LeaseQaPendingKey =
        "TalismanBag.ShougunuPhase1.LeaseQaPending";
    private const string LeaseQaExitKey =
        "TalismanBag.ShougunuPhase1.LeaseQaExit";
    private const string LeaseQaPassedKey =
        "TalismanBag.ShougunuPhase1.LeaseQaPassed";
    private const string LeaseQaResultKey =
        "TalismanBag.ShougunuPhase1.LeaseQaResult";
    private const string LeaseQaCaptureDirectoryKey =
        "TalismanBag.ShougunuPhase1.LeaseQaCaptureDirectory";
    private const string LeaseQaCaptureArgument =
        "-shougunuQaCaptureDir";

    private static int leaseQaInstallAttempts;

    [InitializeOnLoadMethod]
    private static void RegisterLeaseQaEditorHooks()
    {
        EditorApplication.playModeStateChanged -= HandleLeaseQaPlayModeStateChanged;
        EditorApplication.playModeStateChanged += HandleLeaseQaPlayModeStateChanged;
    }

    public static void RunLeaseQaBatch()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            throw new InvalidOperationException(
                PackageId + " lease QA requires Edit Mode.");
        }

        UnityEngine.SceneManagement.Scene scene = EditorSceneManager.OpenScene(
            LeaseQaScenePath,
            OpenSceneMode.Single);
        if (!scene.IsValid() ||
            !string.Equals(scene.path, LeaseQaScenePath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                PackageId + " could not open the authorized sandbox scene.");
        }

        SessionState.SetBool(LeaseQaPendingKey, true);
        SessionState.SetBool(LeaseQaExitKey, false);
        SessionState.SetBool(LeaseQaPassedKey, false);
        SessionState.SetString(LeaseQaResultKey, string.Empty);
        SessionState.SetString(
            LeaseQaCaptureDirectoryKey,
            ReadCommandLineArgument(LeaseQaCaptureArgument));
        leaseQaInstallAttempts = 0;
        Debug.Log($"[{PackageId}] LEASE_QA_ENTER_PLAY scene={LeaseQaScenePath}");
        EditorApplication.EnterPlaymode();
    }

    private static void HandleLeaseQaPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode &&
            SessionState.GetBool(LeaseQaPendingKey, false))
        {
            SessionState.EraseBool(LeaseQaPendingKey);
            leaseQaInstallAttempts = 0;
            EditorApplication.delayCall += InstallLeaseQaRuntimeDriver;
            return;
        }

        if (state != PlayModeStateChange.EnteredEditMode ||
            !SessionState.GetBool(LeaseQaExitKey, false))
        {
            return;
        }

        bool passed = SessionState.GetBool(LeaseQaPassedKey, false);
        string result = SessionState.GetString(
            LeaseQaResultKey,
            "LEASE_QA_FAIL missing result");
        SessionState.EraseBool(LeaseQaExitKey);
        SessionState.EraseBool(LeaseQaPassedKey);
        SessionState.EraseString(LeaseQaResultKey);
        SessionState.EraseString(LeaseQaCaptureDirectoryKey);

        if (passed)
        {
            Debug.Log($"[{PackageId}] {result}");
        }
        else
        {
            Debug.LogError($"[{PackageId}] {result}");
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
                "LEASE_QA_FAIL editor left Play Mode before driver installation");
            return;
        }

        ShougunuPhase1VisualPrototypeController controller =
            FindObjectOfType<ShougunuPhase1VisualPrototypeController>();
        if (controller == null || !controller.initialized)
        {
            leaseQaInstallAttempts++;
            if (leaseQaInstallAttempts < 180)
            {
                EditorApplication.delayCall += InstallLeaseQaRuntimeDriver;
                return;
            }

            CompleteLeaseQaFromPlay(
                false,
                "LEASE_QA_FAIL runtime controller was not attached within 180 editor updates");
            return;
        }

        GameObject driverObject = new GameObject(
            "ShougunuPhase1_LeaseQaRuntimeDriver");
        driverObject.hideFlags = HideFlags.HideAndDontSave;
        LeaseQaRuntimeDriver driver =
            driverObject.AddComponent<LeaseQaRuntimeDriver>();
        driver.Begin(controller);
    }

    private static void CompleteLeaseQaFromPlay(bool passed, string result)
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

    private static string ReadCommandLineArgument(string argumentName)
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

    private bool SimulateMissingPoseFallback(out string failure)
    {
        PoseAsset original = poses[PoseKind.Skill2];
        try
        {
            poses[PoseKind.Skill2] = new PoseAsset
            {
                Calibration = original.Calibration,
                Sprite = poses[PoseKind.Idle].Sprite,
                UsedFallback = true
            };
            ApplyPose(PoseKind.Skill2);
            if (bodyImage.sprite == null ||
                bodyImage.sprite != poses[PoseKind.Idle].Sprite)
            {
                failure = "synthetic missing-pose fallback did not resolve to idle";
                return false;
            }

            failure = string.Empty;
            return true;
        }
        finally
        {
            poses[PoseKind.Skill2] = original;
            ResetVisualOnly();
        }
    }

    private sealed class LeaseQaRuntimeDriver : MonoBehaviour
    {
        private readonly List<string> failures = new List<string>();
        private readonly List<string> evidence = new List<string>();
        private readonly List<string> consoleWarnings = new List<string>();
        private readonly List<string> consoleErrors = new List<string>();
        private ShougunuPhase1VisualPrototypeController controller;
        private string captureDirectory;
        private bool completed;

        public void Begin(ShougunuPhase1VisualPrototypeController target)
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
                controller.gameObject.scene.name == TargetSceneName &&
                controller.name == TargetObjectName &&
                controller.transform.parent != null &&
                controller.transform.parent.name == TargetParentName,
                "runtime controller attached to an unauthorized target");
            Require(
                controller.sourceImage != null &&
                !controller.sourceImage.enabled,
                "authored Image was not safely replaced at runtime");
            Require(
                controller.backRoot != null &&
                controller.motionRoot != null &&
                controller.frontRoot != null,
                "Back/Motion/Front runtime layering was not built");

            controller.StartManual(ActionKind.Idle);
            yield return null;
            ValidateNeutral("1 Idle / Presence");
            evidence.Add("1=IdlePresence");

            yield return ExerciseAction(
                ActionKind.Attack,
                "2 Basic Attack",
                "Impact_Front_CoreGlow",
                0.62f);
            yield return ExerciseAction(
                ActionKind.Hit,
                "3 Hit Feedback",
                "Impact_Front_CoreGlow",
                0.05f);
            yield return ExerciseAction(
                ActionKind.Skill1,
                "4 Skill 1",
                "Skill1_Front_BarrierPanel",
                0.10f);
            yield return ExerciseAction(
                ActionKind.Skill2,
                "5 Skill 2",
                "Skill2_Front_BindingRope",
                0.20f);
            yield return ExerciseAction(
                ActionKind.Skill3,
                "6 Skill 3",
                "Skill3_Back_GroundSealOuter",
                0.42f);

            controller.StartManual(ActionKind.Skill3);
            yield return new WaitForSecondsRealtime(0.18f);
            Require(
                controller.transientObjects.Count > 0,
                "interrupt fixture did not create old-skill VFX");
            controller.StartManual(ActionKind.Skill1);
            yield return null;
            yield return WaitForActionCompletion(
                "Skill3→Skill1 interruption",
                5f);
            ValidateNeutral("Skill3→Skill1 interruption");
            evidence.Add("interruptCleanup=pass");

            int autoBaseline = controller.autoPassCount;
            controller.ToggleAutoShowcase();
            yield return WaitUntil(
                () => controller.autoPassCount > autoBaseline,
                18f,
                "Space Auto Showcase full pass");
            Require(
                controller.autoPassCount > autoBaseline,
                "Space Auto Showcase did not finish a full ordered pass");
            controller.ToggleAutoShowcase();
            yield return null;
            ValidateNeutral("Space Auto Showcase stop");
            evidence.Add("spaceAuto=fullPass");

            controller.StartStabilityStressProbe();
            yield return null;
            yield return WaitUntil(
                () => !controller.stressActive,
                18f,
                "F9 ×20 stability probe");
            Require(
                controller.lastStressPassed,
                "F9 ×20 stability probe did not report PASS");
            ValidateNeutral("F9 ×20 stability probe");
            evidence.Add("f9=20cyclesPass");

            Require(
                controller.SimulateMissingPoseFallback(out string fallbackFailure),
                fallbackFailure);
            ValidateNeutral("missing-resource fallback simulation");
            evidence.Add("missingResourceFallback=pass");

            Require(
                consoleWarnings.Count == 0,
                "runtime Console warnings: " + string.Join(" || ", consoleWarnings));
            Require(
                consoleErrors.Count == 0,
                "runtime Console errors: " + string.Join(" || ", consoleErrors));

            yield return null;
            Finish();
        }

        private IEnumerator ExerciseAction(
            ActionKind action,
            string label,
            string expectedVfxNameFragment,
            float inspectionDelay)
        {
            controller.StartManual(action);
            yield return null;
            yield return new WaitForSecondsRealtime(inspectionDelay);
            Require(
                FindRuntimeObjectContaining(expectedVfxNameFragment),
                label + " did not create expected distinct VFX layer " +
                expectedVfxNameFragment);
            if (action == ActionKind.Skill2 || action == ActionKind.Skill3)
            {
                ValidateNoHardRectangleMasks(label);
                yield return CaptureRenderedFrame(action, label);
            }
            yield return WaitForActionCompletion(label, 6f);
            ValidateNeutral(label);
            evidence.Add(label.Replace(" ", string.Empty) + "=pass");
        }

        private IEnumerator CaptureRenderedFrame(
            ActionKind action,
            string label)
        {
            Require(
                !string.IsNullOrWhiteSpace(captureDirectory),
                label + " rendered capture directory argument was missing");
            if (string.IsNullOrWhiteSpace(captureDirectory))
            {
                yield break;
            }

            Directory.CreateDirectory(captureDirectory);
            string fileName = action == ActionKind.Skill2
                ? "Shougunu_Skill2_BlackMaskRegression.png"
                : "Shougunu_Skill3_BlackMaskRegression.png";
            string capturePath = Path.Combine(captureDirectory, fileName);
            ScreenCapture.CaptureScreenshot(capturePath, 1);
            yield return new WaitForEndOfFrame();
            yield return null;
            yield return null;
            yield return new WaitForSecondsRealtime(0.25f);
            Require(
                File.Exists(capturePath) &&
                new FileInfo(capturePath).Length > 0,
                label + " rendered screenshot was not produced: " + capturePath);
            evidence.Add(fileName + "=captured");
        }

        private IEnumerator WaitForActionCompletion(string label, float timeout)
        {
            yield return WaitUntil(
                () => !controller.actionActive,
                timeout,
                label);
        }

        private IEnumerator WaitUntil(
            Func<bool> predicate,
            float timeout,
            string label)
        {
            float start = Time.realtimeSinceStartup;
            while (!predicate() &&
                   Time.realtimeSinceStartup - start < timeout)
            {
                yield return null;
            }

            Require(predicate(), label + " timed out after " + timeout + "s");
        }

        private bool FindRuntimeObjectContaining(string nameFragment)
        {
            Transform[] transforms =
                controller.targetRect.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name.Contains(nameFragment))
                {
                    return true;
                }
            }

            return false;
        }

        private void ValidateNeutral(string label)
        {
            Require(
                controller.ValidateNeutralState(
                    out string neutralFailure,
                    true),
                label + " neutral validation failed: " + neutralFailure);
            Require(
                controller.bodyFlashImage.color.a <= 0.0001f,
                label + " left body flash visible");
            ValidateNoHardRectangleMasks(label);
        }

        private void ValidateNoHardRectangleMasks(string label)
        {
            Image[] images =
                controller.targetRect.GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null)
                {
                    continue;
                }

                Require(
                    !image.name.Contains("ArtifactShade"),
                    label + " retained an artifact-shade Image capable of a hard rectangle");

                if (!image.gameObject.activeInHierarchy ||
                    !image.enabled ||
                    image.sprite != controller.solidSprite ||
                    image.color.a <= 0.0001f)
                {
                    continue;
                }

                RectTransform rect = image.rectTransform;
                bool fullStretch =
                    (rect.anchorMin - Vector2.zero).sqrMagnitude <= 0.0001f &&
                    (rect.anchorMax - Vector2.one).sqrMagnitude <= 0.0001f &&
                    rect.sizeDelta.sqrMagnitude <= 0.0001f;
                float luminance =
                    image.color.r * 0.2126f +
                    image.color.g * 0.7152f +
                    image.color.b * 0.0722f;
                Require(
                    !fullStretch || luminance > 0.5f,
                    label +
                    " activated a nonzero dark full-stretch solid Image: " +
                    image.name);
            }
        }

        private void CaptureLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Warning)
            {
                consoleWarnings.Add(condition);
            }
            else if (type == LogType.Error ||
                     type == LogType.Exception ||
                     type == LogType.Assert)
            {
                consoleErrors.Add(condition);
            }
        }

        private bool Require(bool condition, string message)
        {
            if (condition)
            {
                return true;
            }

            failures.Add(message);
            return false;
        }

        private void Finish()
        {
            if (completed)
            {
                return;
            }

            completed = true;
            Application.logMessageReceived -= CaptureLog;
            controller.StopPlaybackAndReset();
            bool passed = failures.Count == 0;
            string result = passed
                ? "LEASE_QA_PASS " + string.Join("; ", evidence) +
                  "; runtimeWarnings=0; runtimeErrors=0"
                : "LEASE_QA_FAIL " + string.Join(" || ", failures) +
                  "; runtimeWarnings=" + consoleWarnings.Count +
                  "; runtimeErrors=" + consoleErrors.Count;
            CompleteLeaseQaFromPlay(passed, result);
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= CaptureLog;
            if (!completed && EditorApplication.isPlaying)
            {
                CompleteLeaseQaFromPlay(
                    false,
                    "LEASE_QA_FAIL runtime driver was destroyed before completion");
            }
        }
    }
#endif
}
