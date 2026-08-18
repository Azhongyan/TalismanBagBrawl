using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TalismanBag.V04.ChapterFlow.Chapter1.Presentation;

namespace TalismanBag.BuildSandbox
{
    /// <summary>
    /// One-off, dev-only presentation prototype for evaluating limited-motion enemy art.
    /// It intentionally owns no Enemy/Battle state and installs only in the V0.4 preview scene.
    /// </summary>
    public sealed class BoneHoundVisualPrototypeController : MonoBehaviour
    {
        public const string PackageName = "V0.4-BoneHoundLimitedMotionVfxPrototype01";
        public const string TargetSceneName = "Scene_TalismanBag_V04_BattleSandboxPreview";
        public const string TargetSlotName = "Enemy";

        private const string IdleResourcePath = "Enemy/guciquan_idle";
        private const string AttackResourcePath = "Enemy/guciquan_attack";
        private const string HitResourcePath = "Enemy/guciquan_hit";
        private const string DeathResourcePath = "Enemy/guciquan_death";
        private const string ControlPanelName = "BoneHoundVisualPrototypeControls";

        private static bool bootstrapRegistered;

        private readonly List<Sprite> runtimeSprites = new List<Sprite>();
        private readonly List<Texture2D> runtimeTextures = new List<Texture2D>();
        private readonly List<GameObject> liveEffects = new List<GameObject>();

        private Image slotImage;
        private bool originalSlotImageEnabled;
        private Color originalSlotColor;
        private Sprite originalSlotSprite;

        private RectTransform backVfxRoot;
        private RectTransform motionRoot;
        private RectTransform frontVfxRoot;
        private RectTransform bodyRect;
        private Image bodyImage;
        private Image flashImage;
        private CanvasGroup motionCanvasGroup;
        private Image auraImage;
        private RectTransform auraRect;
        private GameObject controlPanel;
        private Text statusText;
        private Text speedButtonText;

        private Sprite idleSprite;
        private Sprite attackSprite;
        private Sprite hitSprite;
        private Sprite deathSprite;
        private Sprite softCircleSprite;
        private Sprite arcSprite;
        private Sprite solidSprite;
        private Material additiveMaterial;
        private Font controlFont;
        private bool ownsControlFont;

        private Coroutine autoRoutine;
        private Coroutine actionRoutine;
        private bool initialized;
        private bool actionPlaying;
        private float prototypeClock;
        private float playbackSpeed = 1f;

        public bool DevOnly => true;
        public bool OwnsEnemyRuntimeTruth => false;
        public bool WritesFormalFlow => false;
        public bool WritesSaveOrReward => false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RegisterBootstrap()
        {
            if (bootstrapRegistered)
            {
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            bootstrapRegistered = true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InstallIntoActiveScene()
        {
            TryInstall(SceneManager.GetActiveScene());
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryInstall(scene);
        }

        private static void TryInstall(Scene scene)
        {
            if (!scene.IsValid() || !string.Equals(scene.name, TargetSceneName, StringComparison.Ordinal))
            {
                return;
            }

            Image target = UnityEngine.Object.FindObjectsOfType<Image>(true)
                .Where(image => image != null && image.gameObject.scene == scene)
                .Where(image => string.Equals(image.gameObject.name, TargetSlotName, StringComparison.Ordinal))
                .OrderByDescending(image => image.rectTransform.rect.width * image.rectTransform.rect.height)
                .FirstOrDefault();

            if (target == null)
            {
                Debug.LogWarning(
                    $"[{PackageName}] Existing Enemy UI slot was not found. No scene object was created or moved.");
                return;
            }

            if (target.GetComponent<
                    C1AuthoredEnemyPresentationSlotView>() != null)
            {
                Debug.Log(
                    $"[{PackageName}] Authored multi-enemy presentation contract owns this slot; legacy single-slot prototype was not installed.");
                return;
            }

            if (target.GetComponent<BoneHoundVisualPrototypeController>() == null)
            {
                target.gameObject.AddComponent<BoneHoundVisualPrototypeController>();
            }
        }

        private void Awake()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            RestoreExistingSlot();

            if (controlPanel != null)
            {
                Destroy(controlPanel);
            }

            if (additiveMaterial != null)
            {
                Destroy(additiveMaterial);
            }

            foreach (Sprite sprite in runtimeSprites)
            {
                if (sprite != null)
                {
                    Destroy(sprite);
                }
            }

            foreach (Texture2D texture in runtimeTextures)
            {
                if (texture != null)
                {
                    Destroy(texture);
                }
            }

            if (ownsControlFont && controlFont != null)
            {
                Destroy(controlFont);
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            HandleKeyboardInput();
            prototypeClock += Time.unscaledDeltaTime * playbackSpeed;

            if (!actionPlaying)
            {
                ApplyIdleMotion();
            }

            if (auraRect != null && auraImage != null)
            {
                float pulse = 1f + Mathf.Sin(prototypeClock * 2.3f) * 0.05f;
                auraRect.localScale = Vector3.one * pulse;
                Color auraColor = auraImage.color;
                auraColor.a = 0.11f + (Mathf.Sin(prototypeClock * 2.3f) + 1f) * 0.025f;
                auraImage.color = auraColor;
            }
        }

        private void Initialize()
        {
            slotImage = GetComponent<Image>();
            if (slotImage == null)
            {
                return;
            }

            idleSprite = LoadStateSprite(IdleResourcePath);
            attackSprite = LoadStateSprite(AttackResourcePath);
            hitSprite = LoadStateSprite(HitResourcePath);
            deathSprite = LoadStateSprite(DeathResourcePath);

            if (idleSprite == null || attackSprite == null || hitSprite == null || deathSprite == null)
            {
                Debug.LogWarning(
                    $"[{PackageName}] One or more Bone Hound textures are missing. Prototype was not installed.");
                return;
            }

            originalSlotImageEnabled = slotImage.enabled;
            originalSlotColor = slotImage.color;
            originalSlotSprite = slotImage.sprite;

            CreateProceduralVfxResources();
            CreateAdditiveMaterial();
            BuildVisualHierarchy();

            slotImage.enabled = false;
            initialized = true;
            ResetVisual(clearEffects: true);
            StartAutomaticPreview();
            Debug.Log($"[{PackageName}] Installed on existing {TargetSlotName} UI slot. Scene geometry unchanged.");
        }

        private Sprite LoadStateSprite(string resourcePath)
        {
            Sprite importedSprite = Resources.Load<Sprite>(resourcePath);
            if (importedSprite != null)
            {
                return importedSprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(resourcePath);
            if (texture == null)
            {
                return null;
            }

            Sprite runtimeSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            runtimeSprite.name = resourcePath.Replace('/', '_') + "_RuntimeSprite";
            runtimeSprites.Add(runtimeSprite);
            return runtimeSprite;
        }

        private void BuildVisualHierarchy()
        {
            backVfxRoot = CreateRect("BoneHound_VFX_Back", transform);
            motionRoot = CreateRect("BoneHound_MotionRoot", transform);
            frontVfxRoot = CreateRect("BoneHound_VFX_Front", transform);

            bodyImage = CreateImage(
                "BoneHound_Body",
                motionRoot,
                idleSprite,
                Color.white,
                Vector2.zero,
                Vector2.zero,
                additive: false,
                stretch: true);
            bodyRect = bodyImage.rectTransform;

            flashImage = CreateImage(
                "BoneHound_HitFlash",
                motionRoot,
                idleSprite,
                new Color(1f, 0.45f, 0.32f, 0f),
                Vector2.zero,
                Vector2.zero,
                additive: true,
                stretch: true);
            flashImage.enabled = false;

            motionCanvasGroup = motionRoot.gameObject.AddComponent<CanvasGroup>();
            motionCanvasGroup.alpha = 1f;
            motionCanvasGroup.interactable = false;
            motionCanvasGroup.blocksRaycasts = false;

            auraImage = CreateImage(
                "BoneHound_IdleAura",
                backVfxRoot,
                softCircleSprite,
                new Color(0.85f, 0.08f, 0.025f, 0.14f),
                new Vector2(20f, -15f),
                new Vector2(390f, 390f),
                additive: true,
                stretch: false);
            auraRect = auraImage.rectTransform;

            backVfxRoot.SetSiblingIndex(0);
            motionRoot.SetSiblingIndex(1);
            frontVfxRoot.SetSiblingIndex(2);
        }

        private void BuildControlPanel()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            Transform existing = canvas.transform.Find(ControlPanelName);
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }

            controlFont = LoadControlFont();

            controlPanel = new GameObject(
                ControlPanelName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(HorizontalLayoutGroup));
            controlPanel.layer = canvas.gameObject.layer;
            RectTransform panelRect = controlPanel.GetComponent<RectTransform>();
            panelRect.SetParent(canvas.transform, false);
            panelRect.anchorMin = new Vector2(0.5f, 0f);
            panelRect.anchorMax = new Vector2(0.5f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 18f);
            panelRect.sizeDelta = new Vector2(1020f, 72f);

            Image panelImage = controlPanel.GetComponent<Image>();
            panelImage.color = new Color(0.025f, 0.018f, 0.018f, 0.88f);
            panelImage.raycastTarget = true;

            HorizontalLayoutGroup layout = controlPanel.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            statusText = CreateControlText(controlPanel.transform, "骨瓷犬演示：准备中", 212f);
            CreateControlButton(controlPanel.transform, "自动", StartAutomaticPreview);
            CreateControlButton(controlPanel.transform, "待机", ShowIdle);
            CreateControlButton(controlPanel.transform, "攻击", () => PlayManual(PlayAttackRoutine(), "攻击", false));
            CreateControlButton(controlPanel.transform, "技能VFX", () => PlayManual(PlaySkillRoutine(), "技能VFX", false), 106f);
            CreateControlButton(controlPanel.transform, "受击", () => PlayManual(PlayHitRoutine(), "受击", false));
            CreateControlButton(controlPanel.transform, "死亡", () => PlayManual(PlayDeathRoutine(), "死亡", true));
            CreateControlButton(controlPanel.transform, "重置", ShowIdle);
            speedButtonText = CreateControlButton(
                controlPanel.transform,
                "速度×1",
                TogglePlaybackSpeed,
                92f);
            controlPanel.transform.SetAsLastSibling();
        }

        private Font LoadControlFont()
        {
            Font font = null;
            try
            {
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (ArgumentException)
            {
                // Fall through to the OS font used only by this dev-only control strip.
            }

            if (font != null)
            {
                return font;
            }

            font = Font.CreateDynamicFontFromOSFont(
                new[] { "Microsoft YaHei", "SimHei", "Arial" },
                18);
            ownsControlFont = font != null;
            return font;
        }

        private Text CreateControlText(Transform parent, string label, float width)
        {
            GameObject textObject = new GameObject(
                "Status",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text),
                typeof(LayoutElement));
            textObject.layer = controlPanel.layer;
            textObject.transform.SetParent(parent, false);

            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 48f);
            LayoutElement element = textObject.GetComponent<LayoutElement>();
            element.preferredWidth = width;
            element.preferredHeight = 48f;

            Text text = textObject.GetComponent<Text>();
            text.font = controlFont;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = new Color(1f, 0.88f, 0.72f, 1f);
            text.text = label;
            text.raycastTarget = false;
            return text;
        }

        private Text CreateControlButton(
            Transform parent,
            string label,
            UnityEngine.Events.UnityAction action,
            float width = 82f)
        {
            GameObject buttonObject = new GameObject(
                label,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button),
                typeof(LayoutElement));
            buttonObject.layer = controlPanel.layer;
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(width, 48f);
            LayoutElement element = buttonObject.GetComponent<LayoutElement>();
            element.preferredWidth = width;
            element.preferredHeight = 48f;

            Image background = buttonObject.GetComponent<Image>();
            background.color = new Color(0.32f, 0.08f, 0.045f, 0.96f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = background;
            button.onClick.AddListener(action);
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.16f, 1.08f, 1f, 1f);
            colors.pressedColor = new Color(0.82f, 0.72f, 0.65f, 1f);
            button.colors = colors;

            GameObject labelObject = new GameObject(
                "Label",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Text));
            labelObject.layer = controlPanel.layer;
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            Stretch(labelRect);

            Text text = labelObject.GetComponent<Text>();
            text.font = controlFont;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;
            text.raycastTarget = false;
            return text;
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ShowIdle();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                PlayManual(PlayAttackRoutine(), "攻击", false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                PlayManual(PlaySkillRoutine(), "技能VFX", false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                PlayManual(PlayHitRoutine(), "受击", false);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                PlayManual(PlayDeathRoutine(), "死亡", true);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                StartAutomaticPreview();
            }
        }

        private void StartAutomaticPreview()
        {
            StopAllCoroutines();
            autoRoutine = null;
            actionRoutine = null;
            actionPlaying = false;
            ResetVisual(clearEffects: true);
            SetStatus("自动循环");
            autoRoutine = StartCoroutine(AutomaticPreviewRoutine());
        }

        private IEnumerator AutomaticPreviewRoutine()
        {
            while (true)
            {
                actionPlaying = false;
                SetStatus("待机 / Presence");
                yield return WaitPreviewSeconds(1.35f);

                actionPlaying = true;
                SetStatus("攻击 / Lunge");
                yield return PlayAttackRoutine();
                actionPlaying = false;
                yield return WaitPreviewSeconds(0.45f);

                actionPlaying = true;
                SetStatus("技能 / VFX");
                yield return PlaySkillRoutine();
                actionPlaying = false;
                yield return WaitPreviewSeconds(0.45f);

                actionPlaying = true;
                SetStatus("受击 / Hit");
                yield return PlayHitRoutine();
                actionPlaying = false;
                yield return WaitPreviewSeconds(0.5f);

                actionPlaying = true;
                SetStatus("死亡 / Death");
                yield return PlayDeathRoutine();
                yield return WaitPreviewSeconds(0.85f);

                ResetVisual(clearEffects: true);
                actionPlaying = false;
            }
        }

        private void PlayManual(IEnumerator routine, string label, bool holdFinalPose)
        {
            StopAllCoroutines();
            autoRoutine = null;
            actionRoutine = null;
            ResetVisual(clearEffects: true);
            SetStatus(label + " / 手动");
            actionRoutine = StartCoroutine(ManualActionRoutine(routine, holdFinalPose));
        }

        private IEnumerator ManualActionRoutine(IEnumerator routine, bool holdFinalPose)
        {
            actionPlaying = true;
            yield return routine;
            actionRoutine = null;
            actionPlaying = holdFinalPose;
        }

        private void ShowIdle()
        {
            StopAllCoroutines();
            autoRoutine = null;
            actionRoutine = null;
            actionPlaying = false;
            ResetVisual(clearEffects: true);
            SetStatus("待机 / 手动");
        }

        private void TogglePlaybackSpeed()
        {
            playbackSpeed = playbackSpeed > 0.6f ? 0.35f : 1f;
            if (speedButtonText != null)
            {
                speedButtonText.text = playbackSpeed > 0.6f ? "速度×1" : "速度×0.35";
            }
        }

        private IEnumerator PlayAttackRoutine()
        {
            SetPose(attackSprite, new Vector2(0f, 12f));
            Vector2 windupPosition = new Vector2(22f, -2f);
            yield return TweenPreview(0.14f, progress =>
            {
                float eased = Smooth(progress);
                motionRoot.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, windupPosition, eased);
                motionRoot.localScale = Vector3.one * Mathf.Lerp(1f, 0.955f, eased);
                SetRootRotation(Mathf.Lerp(0f, 3.5f, eased));
            });

            SpawnAfterImage(new Color(1f, 0.16f, 0.055f, 0.34f), 0.24f);
            SpawnDust(new Vector2(85f, -112f), new Color(0.42f, 0.23f, 0.14f, 0.5f), 5);
            Vector2 strikePosition = new Vector2(-108f, 8f);
            yield return TweenPreview(0.085f, progress =>
            {
                float eased = EaseOutCubic(progress);
                motionRoot.anchoredPosition = Vector2.LerpUnclamped(windupPosition, strikePosition, eased);
                motionRoot.localScale = Vector3.one * Mathf.Lerp(0.955f, 1.045f, eased);
                SetRootRotation(Mathf.Lerp(3.5f, -3f, eased));
            });

            SpawnSlash(new Vector2(-155f, 18f));
            SpawnImpact(new Vector2(-210f, -5f), new Color(1f, 0.24f, 0.06f, 1f));
            yield return WaitPreviewSeconds(0.065f);

            yield return TweenPreview(0.24f, progress =>
            {
                float eased = Smooth(progress);
                motionRoot.anchoredPosition = Vector2.LerpUnclamped(strikePosition, Vector2.zero, eased);
                motionRoot.localScale = Vector3.one * Mathf.Lerp(1.045f, 1f, eased);
                SetRootRotation(Mathf.Lerp(-3f, 0f, eased));
            });

            SetIdlePose();
        }

        private IEnumerator PlayHitRoutine()
        {
            SetPose(hitSprite, new Vector2(0f, 18f));
            SpawnHitFlash(new Color(1f, 0.42f, 0.18f, 0.95f), 0.22f);
            SpawnImpact(new Vector2(-42f, 12f), new Color(1f, 0.92f, 0.72f, 1f));
            SpawnFragments(new Vector2(0f, -10f), 11);

            yield return TweenPreview(0.29f, progress =>
            {
                float damping = 1f - progress;
                float x = Mathf.Sin(progress * Mathf.PI * 10f) * 22f * damping;
                float y = Mathf.Sin(progress * Mathf.PI * 6f) * 7f * damping;
                motionRoot.anchoredPosition = new Vector2(x, y);
                motionRoot.localScale = Vector3.one * (1f - Mathf.Sin(progress * Mathf.PI) * 0.025f);
                SetRootRotation(Mathf.Sin(progress * Mathf.PI * 8f) * 2.5f * damping);
            });

            yield return TweenPreview(0.18f, progress =>
            {
                float eased = Smooth(progress);
                motionRoot.anchoredPosition = Vector2.LerpUnclamped(motionRoot.anchoredPosition, Vector2.zero, eased);
                motionRoot.localScale = Vector3.LerpUnclamped(motionRoot.localScale, Vector3.one, eased);
                SetRootRotation(Mathf.Lerp(CurrentRootRotation(), 0f, eased));
            });

            SetIdlePose();
        }

        private IEnumerator PlaySkillRoutine()
        {
            SetIdlePose();
            SpawnPulse(backVfxRoot, new Vector2(10f, -8f), 260f, new Color(0.78f, 0.035f, 0.015f, 0.58f), 0.62f);
            yield return WaitPreviewSeconds(0.11f);
            SpawnPulse(backVfxRoot, new Vector2(10f, -8f), 340f, new Color(1f, 0.26f, 0.045f, 0.42f), 0.62f);
            yield return WaitPreviewSeconds(0.11f);
            SpawnPulse(backVfxRoot, new Vector2(10f, -8f), 430f, new Color(1f, 0.66f, 0.22f, 0.3f), 0.68f);

            yield return TweenPreview(0.42f, progress =>
            {
                float pulse = Mathf.Sin(progress * Mathf.PI * 5f) * (1f - progress) * 0.018f;
                motionRoot.localScale = Vector3.one * (1f + progress * 0.035f + pulse);
                motionRoot.anchoredPosition = new Vector2(0f, Mathf.Sin(progress * Mathf.PI * 4f) * 3f);
            });

            SetPose(attackSprite, new Vector2(0f, 12f));
            SpawnAfterImage(new Color(1f, 0.2f, 0.035f, 0.42f), 0.34f);
            yield return TweenPreview(0.11f, progress =>
            {
                float eased = EaseOutCubic(progress);
                motionRoot.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, new Vector2(-48f, 4f), eased);
                motionRoot.localScale = Vector3.one * Mathf.Lerp(1.035f, 1.07f, eased);
                SetRootRotation(Mathf.Lerp(0f, -2f, eased));
            });

            yield return LaunchSkillProjectile();
            SpawnImpact(new Vector2(-350f, 12f), new Color(1f, 0.13f, 0.02f, 1f));
            SpawnFragments(new Vector2(-320f, 0f), 8);
            yield return WaitPreviewSeconds(0.075f);

            yield return TweenPreview(0.22f, progress =>
            {
                float eased = Smooth(progress);
                motionRoot.anchoredPosition = Vector2.LerpUnclamped(new Vector2(-48f, 4f), Vector2.zero, eased);
                motionRoot.localScale = Vector3.one * Mathf.Lerp(1.07f, 1f, eased);
                SetRootRotation(Mathf.Lerp(-2f, 0f, eased));
            });

            SetIdlePose();
        }

        private IEnumerator PlayDeathRoutine()
        {
            SetPose(deathSprite, new Vector2(0f, 6f));
            SpawnHitFlash(new Color(1f, 0.24f, 0.08f, 0.7f), 0.28f);
            SpawnFragments(new Vector2(0f, -25f), 16);
            SpawnDust(new Vector2(-10f, -105f), new Color(0.36f, 0.24f, 0.18f, 0.58f), 11);

            yield return TweenPreview(0.18f, progress =>
            {
                float damping = 1f - progress;
                motionRoot.anchoredPosition = new Vector2(
                    Mathf.Sin(progress * Mathf.PI * 8f) * 12f * damping,
                    -5f * progress);
                SetRootRotation(Mathf.Sin(progress * Mathf.PI * 6f) * 1.5f * damping);
            });

            yield return TweenPreview(0.78f, progress =>
            {
                float eased = Smooth(progress);
                motionRoot.anchoredPosition = new Vector2(0f, Mathf.Lerp(-5f, -24f, eased));
                motionRoot.localScale = Vector3.one * Mathf.Lerp(1f, 0.965f, eased);
                motionCanvasGroup.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01((progress - 0.18f) / 0.82f));
                SetRootRotation(Mathf.Lerp(0f, -2.5f, eased));
            });
        }

        private IEnumerator LaunchSkillProjectile()
        {
            GameObject projectile = new GameObject(
                "BoneHound_SkillProjectile",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            projectile.layer = gameObject.layer;
            RectTransform projectileRect = projectile.GetComponent<RectTransform>();
            projectileRect.SetParent(frontVfxRoot, false);
            projectileRect.anchorMin = projectileRect.anchorMax = new Vector2(0.5f, 0.5f);
            projectileRect.pivot = new Vector2(0.5f, 0.5f);
            projectileRect.sizeDelta = new Vector2(78f, 78f);

            Image projectileImage = projectile.GetComponent<Image>();
            projectileImage.sprite = softCircleSprite;
            projectileImage.color = new Color(1f, 0.18f, 0.025f, 0.9f);
            projectileImage.material = additiveMaterial;
            projectileImage.raycastTarget = false;
            liveEffects.Add(projectile);

            Vector2 start = new Vector2(-92f, 18f);
            Vector2 end = new Vector2(-350f, 12f);
            float trailTimer = 0f;
            yield return TweenPreview(0.25f, progress =>
            {
                float eased = EaseOutCubic(progress);
                projectileRect.anchoredPosition = Vector2.LerpUnclamped(start, end, eased);
                float pulse = 1f + Mathf.Sin(progress * Mathf.PI * 6f) * 0.12f;
                projectileRect.localScale = Vector3.one * pulse;
                trailTimer += Time.unscaledDeltaTime * playbackSpeed;
                if (trailTimer >= 0.035f)
                {
                    trailTimer = 0f;
                    SpawnPulse(
                        frontVfxRoot,
                        projectileRect.anchoredPosition,
                        62f,
                        new Color(0.95f, 0.08f, 0.02f, 0.32f),
                        0.22f);
                }
            });

            RemoveEffect(projectile);
        }

        private void SpawnHitFlash(Color color, float duration)
        {
            flashImage.sprite = bodyImage.sprite;
            flashImage.rectTransform.anchoredPosition = bodyRect.anchoredPosition;
            flashImage.enabled = true;
            flashImage.color = color;
            StartCoroutine(FadeHitFlash(duration, color));
        }

        private IEnumerator FadeHitFlash(float duration, Color startColor)
        {
            yield return TweenPreview(duration, progress =>
            {
                Color color = startColor;
                color.a = startColor.a * (1f - Smooth(progress));
                if (flashImage != null)
                {
                    flashImage.color = color;
                }
            });

            if (flashImage != null)
            {
                flashImage.enabled = false;
            }
        }

        private void SpawnAfterImage(Color color, float duration)
        {
            Image afterImage = CreateImage(
                "BoneHound_AfterImage",
                frontVfxRoot,
                bodyImage.sprite,
                color,
                motionRoot.anchoredPosition + bodyRect.anchoredPosition,
                Vector2.zero,
                additive: true,
                stretch: true);
            afterImage.rectTransform.localScale = motionRoot.localScale;
            afterImage.rectTransform.localRotation = motionRoot.localRotation;
            liveEffects.Add(afterImage.gameObject);
            StartCoroutine(FadeEffectImage(afterImage, duration, 1.08f));
        }

        private void SpawnSlash(Vector2 position)
        {
            Image slash = CreateImage(
                "BoneHound_AttackArc",
                frontVfxRoot,
                arcSprite,
                new Color(1f, 0.34f, 0.055f, 0.95f),
                position,
                new Vector2(290f, 290f),
                additive: true,
                stretch: false);
            slash.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 18f);
            slash.rectTransform.localScale = Vector3.one * 0.42f;
            liveEffects.Add(slash.gameObject);
            StartCoroutine(FadeEffectImage(slash, 0.24f, 1.45f));
        }

        private void SpawnImpact(Vector2 position, Color color)
        {
            SpawnPulse(frontVfxRoot, position, 150f, color, 0.28f);
            for (int index = 0; index < 10; index++)
            {
                float angle = index * 36f + 8f;
                Image ray = CreateImage(
                    "BoneHound_ImpactRay",
                    frontVfxRoot,
                    solidSprite,
                    new Color(color.r, color.g, color.b, 0.86f),
                    position,
                    new Vector2(128f, 8f),
                    additive: true,
                    stretch: false);
                ray.rectTransform.pivot = new Vector2(0f, 0.5f);
                ray.rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
                ray.rectTransform.localScale = new Vector3(0.08f, 1f, 1f);
                liveEffects.Add(ray.gameObject);
                StartCoroutine(AnimateImpactRay(ray, 0.27f));
            }
        }

        private IEnumerator AnimateImpactRay(Image ray, float duration)
        {
            Color start = ray.color;
            yield return TweenPreview(duration, progress =>
            {
                if (ray == null)
                {
                    return;
                }

                float eased = EaseOutCubic(progress);
                ray.rectTransform.localScale = new Vector3(Mathf.Lerp(0.08f, 1f, eased), 1f, 1f);
                Color color = start;
                color.a = start.a * (1f - Smooth(progress));
                ray.color = color;
            });

            if (ray != null)
            {
                RemoveEffect(ray.gameObject);
            }
        }

        private void SpawnPulse(
            RectTransform parent,
            Vector2 position,
            float size,
            Color color,
            float duration)
        {
            Image pulse = CreateImage(
                "BoneHound_VfxPulse",
                parent,
                softCircleSprite,
                color,
                position,
                new Vector2(size, size),
                additive: true,
                stretch: false);
            pulse.rectTransform.localScale = Vector3.one * 0.18f;
            liveEffects.Add(pulse.gameObject);
            StartCoroutine(FadeEffectImage(pulse, duration, 1.38f));
        }

        private void SpawnFragments(Vector2 origin, int count)
        {
            for (int index = 0; index < count; index++)
            {
                float angle = Mathf.Lerp(35f, 145f, UnityEngine.Random.value);
                float speed = UnityEngine.Random.Range(85f, 210f);
                Vector2 velocity = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad)) * speed;
                if (index % 2 == 0)
                {
                    velocity.x *= -1f;
                }

                Image fragment = CreateImage(
                    "BoneHound_BoneFragment",
                    frontVfxRoot,
                    solidSprite,
                    index % 3 == 0
                        ? new Color(0.7f, 0.08f, 0.03f, 0.92f)
                        : new Color(0.92f, 0.85f, 0.69f, 0.96f),
                    origin + UnityEngine.Random.insideUnitCircle * 26f,
                    new Vector2(UnityEngine.Random.Range(9f, 25f), UnityEngine.Random.Range(5f, 13f)),
                    additive: false,
                    stretch: false);
                fragment.rectTransform.localRotation =
                    Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 180f));
                liveEffects.Add(fragment.gameObject);
                StartCoroutine(AnimateFragment(
                    fragment,
                    velocity,
                    UnityEngine.Random.Range(-310f, 310f),
                    UnityEngine.Random.Range(0.48f, 0.82f)));
            }
        }

        private IEnumerator AnimateFragment(Image fragment, Vector2 velocity, float angularVelocity, float duration)
        {
            Vector2 start = fragment.rectTransform.anchoredPosition;
            Color startColor = fragment.color;
            yield return TweenPreview(duration, progress =>
            {
                if (fragment == null)
                {
                    return;
                }

                float seconds = duration * progress;
                Vector2 gravity = Vector2.down * 300f * seconds * seconds * 0.5f;
                fragment.rectTransform.anchoredPosition = start + velocity * seconds + gravity;
                fragment.rectTransform.localRotation =
                    Quaternion.Euler(0f, 0f, angularVelocity * seconds);
                Color color = startColor;
                color.a = startColor.a * (1f - Mathf.Clamp01((progress - 0.55f) / 0.45f));
                fragment.color = color;
            });

            if (fragment != null)
            {
                RemoveEffect(fragment.gameObject);
            }
        }

        private void SpawnDust(Vector2 origin, Color color, int count)
        {
            for (int index = 0; index < count; index++)
            {
                Image dust = CreateImage(
                    "BoneHound_Dust",
                    backVfxRoot,
                    softCircleSprite,
                    color,
                    origin + new Vector2(UnityEngine.Random.Range(-70f, 70f), UnityEngine.Random.Range(-8f, 24f)),
                    Vector2.one * UnityEngine.Random.Range(46f, 105f),
                    additive: false,
                    stretch: false);
                dust.rectTransform.localScale = Vector3.one * UnityEngine.Random.Range(0.35f, 0.65f);
                liveEffects.Add(dust.gameObject);
                StartCoroutine(AnimateDust(
                    dust,
                    new Vector2(UnityEngine.Random.Range(-28f, 28f), UnityEngine.Random.Range(30f, 75f)),
                    UnityEngine.Random.Range(0.55f, 0.9f)));
            }
        }

        private IEnumerator AnimateDust(Image dust, Vector2 drift, float duration)
        {
            Vector2 start = dust.rectTransform.anchoredPosition;
            Vector3 startScale = dust.rectTransform.localScale;
            Color startColor = dust.color;
            yield return TweenPreview(duration, progress =>
            {
                if (dust == null)
                {
                    return;
                }

                float eased = Smooth(progress);
                dust.rectTransform.anchoredPosition = start + drift * eased;
                dust.rectTransform.localScale = Vector3.LerpUnclamped(startScale, Vector3.one * 1.25f, eased);
                Color color = startColor;
                color.a = startColor.a * (1f - eased);
                dust.color = color;
            });

            if (dust != null)
            {
                RemoveEffect(dust.gameObject);
            }
        }

        private IEnumerator FadeEffectImage(Image image, float duration, float targetScale)
        {
            Color start = image.color;
            Vector3 startScale = image.rectTransform.localScale;
            yield return TweenPreview(duration, progress =>
            {
                if (image == null)
                {
                    return;
                }

                float eased = EaseOutCubic(progress);
                image.rectTransform.localScale =
                    Vector3.LerpUnclamped(startScale, Vector3.one * targetScale, eased);
                Color color = start;
                color.a = start.a * (1f - Smooth(progress));
                image.color = color;
            });

            if (image != null)
            {
                RemoveEffect(image.gameObject);
            }
        }

        private IEnumerator TweenPreview(float duration, Action<float> update)
        {
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.001f, duration);
            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime * playbackSpeed;
                update(Mathf.Clamp01(elapsed / safeDuration));
                yield return null;
            }

            update(1f);
        }

        private IEnumerator WaitPreviewSeconds(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime * playbackSpeed;
                yield return null;
            }
        }

        private void ApplyIdleMotion()
        {
            float floatWave = Mathf.Sin(prototypeClock * Mathf.PI * 2f / 2.15f);
            float breathWave = Mathf.Sin(prototypeClock * Mathf.PI * 2f / 1.65f);
            motionRoot.anchoredPosition = new Vector2(0f, floatWave * 7.5f);
            motionRoot.localScale = Vector3.one * (1f + breathWave * 0.011f);
            SetRootRotation(Mathf.Sin(prototypeClock * 1.35f) * 0.55f);
        }

        private void SetIdlePose()
        {
            SetPose(idleSprite, Vector2.zero);
            motionRoot.anchoredPosition = Vector2.zero;
            motionRoot.localScale = Vector3.one;
            SetRootRotation(0f);
            motionCanvasGroup.alpha = 1f;
        }

        private void SetPose(Sprite sprite, Vector2 poseAnchorCorrection)
        {
            bodyImage.sprite = sprite;
            bodyRect.anchoredPosition = poseAnchorCorrection;
            flashImage.sprite = sprite;
            flashImage.rectTransform.anchoredPosition = poseAnchorCorrection;
        }

        private void ResetVisual(bool clearEffects)
        {
            if (clearEffects)
            {
                ClearEffects();
            }

            SetIdlePose();
            bodyImage.color = Color.white;
            flashImage.enabled = false;
            flashImage.color = new Color(1f, 0.45f, 0.32f, 0f);
            if (auraImage != null)
            {
                auraImage.enabled = true;
            }
        }

        private void ClearEffects()
        {
            foreach (GameObject effect in liveEffects)
            {
                if (effect != null)
                {
                    Destroy(effect);
                }
            }

            liveEffects.Clear();
        }

        private void RemoveEffect(GameObject effect)
        {
            liveEffects.Remove(effect);
            if (effect != null)
            {
                Destroy(effect);
            }
        }

        private void SetStatus(string value)
        {
            if (statusText != null)
            {
                statusText.text = "骨瓷犬：" + value;
            }
        }

        private void SetRootRotation(float degrees)
        {
            motionRoot.localRotation = Quaternion.Euler(0f, 0f, degrees);
        }

        private float CurrentRootRotation()
        {
            float angle = motionRoot.localEulerAngles.z;
            return angle > 180f ? angle - 360f : angle;
        }

        private void RestoreExistingSlot()
        {
            if (slotImage == null)
            {
                return;
            }

            slotImage.sprite = originalSlotSprite;
            slotImage.color = originalSlotColor;
            slotImage.enabled = originalSlotImageEnabled;
        }

        private RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject root = new GameObject(objectName, typeof(RectTransform));
            root.layer = gameObject.layer;
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            return rect;
        }

        private Image CreateImage(
            string objectName,
            Transform parent,
            Sprite sprite,
            Color color,
            Vector2 anchoredPosition,
            Vector2 size,
            bool additive,
            bool stretch)
        {
            GameObject imageObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.layer = gameObject.layer;
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            if (stretch)
            {
                Stretch(rect);
                rect.anchoredPosition = anchoredPosition;
            }
            else
            {
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = size;
            }

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            image.preserveAspect = false;
            if (additive && additiveMaterial != null)
            {
                image.material = additiveMaterial;
            }

            return image;
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

        private void CreateAdditiveMaterial()
        {
            Shader shader = Shader.Find("TalismanBag/UI/CellGlowAdditive");
            if (shader == null)
            {
                Debug.LogWarning(
                    $"[{PackageName}] UI additive shader was not found. VFX will use normal alpha blending.");
                return;
            }

            additiveMaterial = new Material(shader)
            {
                name = "BoneHoundVisualPrototype_Additive_Runtime",
                hideFlags = HideFlags.DontSave
            };
            if (additiveMaterial.HasProperty("_GlowIntensity"))
            {
                additiveMaterial.SetFloat("_GlowIntensity", 1.45f);
            }
        }

        private void CreateProceduralVfxResources()
        {
            softCircleSprite = CreateSoftCircleSprite(64);
            arcSprite = CreateArcSprite(128);
            solidSprite = CreateSolidSprite();
        }

        private Sprite CreateSoftCircleSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "BoneHoundPrototype_SoftCircle",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 1.65f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return RegisterProceduralSprite(texture, "BoneHoundPrototype_SoftCircleSprite");
        }

        private Sprite CreateArcSprite(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "BoneHoundPrototype_Arc",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            Color[] pixels = new Color[size * size];
            float center = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - center) / center;
                    float dy = (y - center) / center;
                    float radius = Mathf.Sqrt(dx * dx + dy * dy);
                    float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                    if (angle < 0f)
                    {
                        angle += 360f;
                    }

                    bool angularRange = angle >= 115f || angle <= 22f;
                    float ring = 1f - Mathf.Clamp01(Mathf.Abs(radius - 0.72f) / 0.16f);
                    float alpha = angularRange ? ring * Mathf.Clamp01((0.98f - radius) * 8f) : 0f;
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return RegisterProceduralSprite(texture, "BoneHoundPrototype_ArcSprite");
        }

        private Sprite CreateSolidSprite()
        {
            Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "BoneHoundPrototype_Solid",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            Color[] pixels = Enumerable.Repeat(Color.white, 16).ToArray();
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return RegisterProceduralSprite(texture, "BoneHoundPrototype_SolidSprite");
        }

        private Sprite RegisterProceduralSprite(Texture2D texture, string spriteName)
        {
            runtimeTextures.Add(texture);
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = spriteName;
            sprite.hideFlags = HideFlags.DontSave;
            runtimeSprites.Add(sprite);
            return sprite;
        }

        private static float Smooth(float value)
        {
            return value * value * (3f - 2f * value);
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1f - value;
            return 1f - inverse * inverse * inverse;
        }
    }
}
