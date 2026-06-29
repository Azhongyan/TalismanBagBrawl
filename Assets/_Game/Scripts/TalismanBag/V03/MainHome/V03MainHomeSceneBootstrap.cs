using System.Collections.Generic;
using TalismanBag.V02.UI;
using TalismanBag.V03.Forge;
using TalismanBag.V03.Navigation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TalismanBag.V03.MainHome
{
    [ExecuteAlways]
    public sealed class V03MainHomeSceneBootstrap : MonoBehaviour
    {
        private const float BottomNavWidth = 860f;
        private const float BottomNavHeight = 124f;
        private const float BottomNavMinBottomInset = 56f;
        private const float BottomNavMaxBottomInset = 104f;
        private const float BottomNavButtonStep = 168f;
        private const float BottomNavButtonWidth = 148f;
        private const float BottomNavButtonHeight = 82f;
        private const string HomeFullBackgroundResourcePath =
            "V03/MainHome/HomeFullBackground";
        private const string HomeFullBackgroundSlotName =
            "FullBackgroundImageSlot";
        private const string HomeFullBackgroundUnderlayName =
            "FullBackgroundBlackUnderlay";
        private const string BottomNavRootName = "BottomNavBar_Root";
        private const float HomeFullBackgroundSize = 2200f;

        [SerializeField] private MainHomeGreyboxPanel homePanel;
        [SerializeField] private V03NavigationFlowController navigation;
        [SerializeField] private string resourceSummary =
            "灵石 0　符纸 0\n朱砂 0　初阶符胚 0\n修为 0";
        [SerializeField] private string status = "小店已开门，选择店内区域查看。";

        private V03ForgeFirstUpgradeGuideController fallbackForgeGuide;
        private GameObject fallbackBottomNavRoot;
        private static Sprite homeFullBackgroundSprite;
#if UNITY_EDITOR
        private void OnEnable()
        {
        }

        private void OnValidate()
        {
        }

        private void EnsureEditorPreview()
        {
            ResolveHomePanelIfNeeded();
            if (homePanel == null)
            {
                return;
            }

            EnsureFullBackgroundSlot();
            homePanel.ApplyV03MainHomeUiueLayout();
            EnsureFallbackBottomNav(false);
        }

        [ContextMenu("Refresh V0.3 Main Home Preview")]
        public void RefreshEditorPreview()
        {
            if (Application.isPlaying)
            {
                return;
            }

            EnsureEditorPreview();
            UnityEditor.EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }

        private void ResolveHomePanelIfNeeded()
        {
            if (homePanel != null)
            {
                return;
            }

            homePanel = FindObjectOfType<MainHomeGreyboxPanel>(true);
        }
#endif

        private void Start()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
#endif
            if (homePanel == null)
            {
                Debug.LogError("[V0.3-MainHome] MainHomeRoot reference is missing.", this);
                return;
            }

            ValidateLockedMainHomeScene();
            homePanel.ApplyV03MainHomeUiueLayout();

            if (navigation == null)
            {
                navigation = GetComponent<V03NavigationFlowController>();
            }

            if (navigation == null)
            {
                navigation = FindObjectOfType<V03NavigationFlowController>(true);
            }

            if (navigation != null)
            {
                navigation.Initialize(homePanel, resourceSummary, status);
                return;
            }

            homePanel.Show(
                MainHomeGreyboxPanel.HomeTitle,
                resourceSummary,
                status,
                OnRefineRequested,
                OnTrialRequested,
                null);
            EnsureFallbackBottomNav(true);
            ResolveExistingForgeGuide()?.OnHomeShown();
        }

        private void OnRefineRequested()
        {
            ResolveExistingForgeGuide()?.HideGuideSlot();
            LoadScene(
                V03NavigationFlowController.UpgradeScenePath,
                V03NavigationFlowController.UpgradeSceneName,
                $"[V0.3-MainHome] Upgrade scene is missing from Build Settings: {V03NavigationFlowController.UpgradeScenePath}");
        }

        private void OnTrialRequested()
        {
            V03ForgeFirstUpgradeGuideController guide = ResolveExistingForgeGuide();
            if (guide != null && guide.ShouldBlockTrialUntilFirstUpgrade())
            {
                guide.ShowFirstUpgradeHomeGuide();
                return;
            }

            guide?.HideGuideSlot();
            LoadScene(
                V03NavigationFlowController.TrialScenePath,
                V03NavigationFlowController.TrialSceneName,
                $"[V0.3-MainHome] Trial scene is missing from Build Settings: {V03NavigationFlowController.TrialScenePath}");
        }

        private V03ForgeFirstUpgradeGuideController ResolveExistingForgeGuide()
        {
            if (fallbackForgeGuide != null)
            {
                return fallbackForgeGuide;
            }

            fallbackForgeGuide = GetComponent<V03ForgeFirstUpgradeGuideController>();
            if (fallbackForgeGuide == null)
            {
                Debug.LogWarning(
                    "[V0.3-MainHomeRuntimeLock] Forge guide component is missing; " +
                    "runtime component creation is disabled.",
                    this);
                return null;
            }

            fallbackForgeGuide.Initialize(null, homePanel, null);
            return fallbackForgeGuide;
        }

        private void EnsureFallbackBottomNav(bool bindButtons)
        {
            Canvas canvas = homePanel.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Transform existing = FindReusableBottomNavRoot(canvas.transform);
                if (existing == null)
                {
                    Debug.LogError(
                        "[V0.3-MainHomeRuntimeLock] BottomNavBar_Root is missing; " +
                        "runtime fallback bottom nav creation is disabled.",
                        this);
                    fallbackBottomNavRoot = null;
                    return;
                }

                fallbackBottomNavRoot = existing.gameObject;
                if (bindButtons)
                {
                    BindExistingFallbackBottomNav(existing);
                }

                return;
            }

            if (fallbackBottomNavRoot == null)
            {
                Transform existing = FindReusableBottomNavRoot(canvas.transform);
                fallbackBottomNavRoot = existing != null
                    ? existing.gameObject
                    : new GameObject(BottomNavRootName, typeof(RectTransform), typeof(Image));
            }

            bool createdBottomNav = fallbackBottomNavRoot.transform.parent == null;
            if (createdBottomNav)
            {
                fallbackBottomNavRoot.transform.SetParent(canvas.transform, false);
                fallbackBottomNavRoot.SetActive(true);
            }

            RectTransform rect = fallbackBottomNavRoot.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = fallbackBottomNavRoot.AddComponent<RectTransform>();
            }

            if (createdBottomNav)
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, ResolveBottomNavInset(canvas));
                rect.sizeDelta = new Vector2(BottomNavWidth, BottomNavHeight);
            }

            Image background = fallbackBottomNavRoot.GetComponent<Image>();
            bool addedBackground = false;
            if (background == null)
            {
                background = fallbackBottomNavRoot.AddComponent<Image>();
                addedBackground = true;
            }

            if (createdBottomNav || addedBackground)
            {
                background.color = Color.clear;
                background.raycastTarget = false;
            }

            if (fallbackBottomNavRoot.transform.childCount > 0)
            {
                if (!bindButtons)
                {
                    return;
                }

                BindExistingFallbackBottomNav(fallbackBottomNavRoot.transform);
                return;
            }

            CreateFallbackNavButton(rect, "BottomNavHomeButton", "首页", -BottomNavButtonStep * 2f, () =>
            {
                homePanel.Show(
                    MainHomeGreyboxPanel.HomeTitle,
                    resourceSummary,
                    status,
                    OnRefineRequested,
                    OnTrialRequested,
                    null);
                ResolveExistingForgeGuide()?.OnHomeShown();
            });
            CreateFallbackNavButton(rect, "BottomNavRefineButton", "养成", -BottomNavButtonStep, OnRefineRequested);
            CreateFallbackNavButton(rect, "BottomNavTrialButton", "试炼", 0f, OnTrialRequested);
            CreateFallbackNavButton(rect, "BottomNavExploreButton", "探索", BottomNavButtonStep, () =>
                Debug.Log("[V0.3-MainHome] Explore entry is reserved for a later package.", this));
            CreateFallbackNavButton(rect, "BottomNavMoreButton", "更多", BottomNavButtonStep * 2f, () =>
                Debug.Log("[V0.3-MainHome] More entry is reserved for a later package.", this));

        }

        private static Transform FindReusableBottomNavRoot(Transform canvasTransform)
        {
            if (canvasTransform == null)
            {
                return null;
            }

            Transform keep = null;
            Transform[] candidates = canvasTransform.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < candidates.Length; i++)
            {
                Transform candidate = candidates[i];
                if (candidate == null || candidate.name != BottomNavRootName)
                {
                    continue;
                }

                if (keep == null || ShouldPreferBottomNavRoot(candidate, keep, canvasTransform))
                {
                    keep = candidate;
                }
            }

            ReportDuplicateBottomNavRoots(candidates, keep);
            return keep;
        }

        private static bool ShouldPreferBottomNavRoot(
            Transform candidate,
            Transform current,
            Transform canvasTransform)
        {
            bool candidateNested = candidate.parent != canvasTransform;
            bool currentNested = current.parent != canvasTransform;
            if (candidateNested != currentNested)
            {
                return candidateNested;
            }

            return candidate.gameObject.activeInHierarchy && !current.gameObject.activeInHierarchy;
        }

        private void BindExistingFallbackBottomNav(Transform parent)
        {
            BindFallbackNavButton(parent, "BottomNavHomeButton", () =>
            {
                homePanel.Show(
                    MainHomeGreyboxPanel.HomeTitle,
                    resourceSummary,
                    status,
                    OnRefineRequested,
                    OnTrialRequested,
                    null);
                ResolveExistingForgeGuide()?.OnHomeShown();
            });
            BindFallbackNavButton(parent, "BottomNavRefineButton", OnRefineRequested);
            BindFallbackNavButton(parent, "BottomNavTrialButton", OnTrialRequested);
            BindFallbackNavButton(parent, "BottomNavExploreButton", () =>
                Debug.Log("[V0.3-MainHome] Explore entry is reserved for a later package.", this));
            BindFallbackNavButton(parent, "BottomNavMoreButton", () =>
                Debug.Log("[V0.3-MainHome] More entry is reserved for a later package.", this));
        }

        private static void BindFallbackNavButton(
            Transform parent,
            string objectName,
            UnityEngine.Events.UnityAction onClick)
        {
            Transform target = parent.Find(objectName);
            Button button = target != null ? target.GetComponent<Button>() : null;
            if (button != null && onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
        }

        private static void CreateFallbackNavButton(
            Transform parent,
            string objectName,
            string label,
            float x,
            UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(x, 0f);
            rect.sizeDelta = new Vector2(BottomNavButtonWidth, BottomNavButtonHeight);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.12f, 0.15f, 0.14f, 0.98f);

            Button button = buttonObject.GetComponent<Button>();
            if (onClick != null && Application.isPlaying)
            {
                button.onClick.AddListener(onClick);
            }

            GameObject textObject = new("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 24;
            text.fontStyle = FontStyle.Bold;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            text.text = label;
        }

        private static float ResolveBottomNavInset(Canvas canvas)
        {
            float scaleFactor = canvas != null && canvas.scaleFactor > 0f ? canvas.scaleFactor : 1f;
            float safeBottom = Screen.safeArea.yMin / scaleFactor;
            return Mathf.Clamp(
                safeBottom + 18f,
                BottomNavMinBottomInset,
                BottomNavMaxBottomInset);
        }

        private void EnsureFullBackgroundSlot()
        {
            if (Application.isPlaying)
            {
                ValidateLockedMainHomeScene();
                return;
            }

            Canvas canvas = homePanel.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                return;
            }

            GameObject underlayObject = EnsureFullBackgroundUnderlay(canvas.transform, out _);
            GameObject slotObject = EnsureSingleFullBackgroundSlot(canvas.transform, out bool createdSlot);
            if (createdSlot)
            {
                slotObject.transform.SetParent(canvas.transform, false);
                slotObject.transform.SetSiblingIndex(
                    Mathf.Min(underlayObject.transform.GetSiblingIndex() + 1, canvas.transform.childCount - 1));
                slotObject.SetActive(true);
            }

            RectTransform rect = slotObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = slotObject.AddComponent<RectTransform>();
            }

            if (createdSlot)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(HomeFullBackgroundSize, HomeFullBackgroundSize);
            }

            Image image = slotObject.GetComponent<Image>();
            if (image == null && createdSlot)
            {
                image = slotObject.AddComponent<Image>();
            }

            if (createdSlot && image != null)
            {
                Sprite sprite = GetHomeFullBackgroundSprite();
                image.sprite = sprite;
                image.type = Image.Type.Simple;
                image.preserveAspect = false;
                image.color = sprite != null ? Color.white : Color.clear;
                image.raycastTarget = false;
            }
        }

        private static GameObject EnsureFullBackgroundUnderlay(Transform canvasTransform, out bool createdUnderlay)
        {
            createdUnderlay = false;
            Transform existing = canvasTransform.Find(HomeFullBackgroundUnderlayName);
            if (existing == null)
            {
                foreach (Transform candidate in canvasTransform.GetComponentsInChildren<Transform>(true))
                {
                    if (candidate.name == HomeFullBackgroundUnderlayName)
                    {
                        existing = candidate;
                        break;
                    }
                }
            }

            GameObject underlayObject;
            if (existing != null)
            {
                underlayObject = existing.gameObject;
            }
            else
            {
                createdUnderlay = true;
                underlayObject = new GameObject(HomeFullBackgroundUnderlayName, typeof(RectTransform), typeof(Image));
                underlayObject.transform.SetParent(canvasTransform, false);
                underlayObject.transform.SetAsFirstSibling();
                underlayObject.SetActive(true);
            }

            RectTransform rect = underlayObject.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = underlayObject.AddComponent<RectTransform>();
            }

            if (createdUnderlay)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
            }

            Image image = underlayObject.GetComponent<Image>();
            bool addedImage = false;
            if (image == null)
            {
                image = underlayObject.AddComponent<Image>();
                addedImage = true;
            }

            if (createdUnderlay || addedImage)
            {
                image.sprite = null;
                image.color = Color.black;
                image.raycastTarget = false;
            }
            return underlayObject;
        }

        private static GameObject EnsureSingleFullBackgroundSlot(Transform canvasTransform, out bool createdSlot)
        {
            createdSlot = false;
            Transform keep = canvasTransform.Find(HomeFullBackgroundSlotName);
            Transform[] candidates = canvasTransform.GetComponentsInChildren<Transform>(true);
            if (keep == null)
            {
                foreach (Transform candidate in candidates)
                {
                    if (candidate.name == HomeFullBackgroundSlotName)
                    {
                        keep = candidate;
                        break;
                    }
                }
            }

            if (keep == null)
            {
                createdSlot = true;
                return new GameObject(HomeFullBackgroundSlotName, typeof(RectTransform), typeof(Image));
            }

            return keep.gameObject;
        }

        private void ValidateLockedMainHomeScene()
        {
            Canvas canvas = homePanel != null ? homePanel.GetComponentInParent<Canvas>(true) : null;
            if (canvas == null)
            {
                Debug.LogError(
                    "[V0.3-MainHomeRuntimeLock] Canvas is missing; locked scene validation cannot run.",
                    this);
                return;
            }

            Transform underlay = ValidateUniqueSceneObject(canvas.transform, HomeFullBackgroundUnderlayName);
            Transform slot = ValidateUniqueSceneObject(canvas.transform, HomeFullBackgroundSlotName);
            ValidateUniqueSceneObject(canvas.transform, BottomNavRootName);

            if (underlay != null)
            {
                Image image = underlay.GetComponent<Image>();
                if (image == null || !image.enabled)
                {
                    Debug.LogError(
                        "[V0.3-MainHomeRuntimeLock] FullBackgroundBlackUnderlay must keep an enabled Image.",
                        this);
                }
                else if (image.raycastTarget)
                {
                    Debug.LogError(
                        "[V0.3-MainHomeRuntimeLock] FullBackgroundBlackUnderlay must not block input.",
                        this);
                }
            }

            if (slot != null)
            {
                Image image = slot.GetComponent<Image>();
                if (image == null || !image.enabled)
                {
                    Debug.LogError(
                        "[V0.3-MainHomeRuntimeLock] FullBackgroundImageSlot must keep an enabled Image.",
                        this);
                }
                else if (image.raycastTarget)
                {
                    Debug.LogError(
                        "[V0.3-MainHomeRuntimeLock] FullBackgroundImageSlot must not block input.",
                        this);
                }
            }

            Image rootImage = homePanel.GetComponent<Image>();
            if (rootImage != null && rootImage.enabled)
            {
                Debug.LogError(
                    "[V0.3-MainHomeRuntimeLock] MainHomeRoot Image must stay disabled.",
                    this);
            }

            Outline rootOutline = homePanel.GetComponent<Outline>();
            if (rootOutline != null && rootOutline.enabled)
            {
                Debug.LogError(
                    "[V0.3-MainHomeRuntimeLock] MainHomeRoot Outline must stay disabled.",
                    this);
            }
        }

        private Transform ValidateUniqueSceneObject(Transform root, string objectName)
        {
            List<Transform> matches = FindNamedChildren(root, objectName);
            if (matches.Count == 0)
            {
                Debug.LogError(
                    $"[V0.3-MainHomeRuntimeLock] {objectName} is missing; runtime creation is disabled.",
                    this);
                return null;
            }

            if (matches.Count > 1)
            {
                Debug.LogError(
                    $"[V0.3-MainHomeRuntimeLock] Scene contains {matches.Count} {objectName} objects: " +
                    string.Join(", ", matches.ConvertAll(BuildTransformPath)),
                    this);
            }

            return matches[0];
        }

        private static List<Transform> FindNamedChildren(Transform root, string objectName)
        {
            List<Transform> matches = new();
            if (root == null)
            {
                return matches;
            }

            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                if (candidate != null && candidate.name == objectName)
                {
                    matches.Add(candidate);
                }
            }

            return matches;
        }

        private static void ReportDuplicateBottomNavRoots(Transform[] candidates, Transform keep)
        {
            if (!Application.isPlaying || keep == null)
            {
                return;
            }

            List<string> duplicates = new();
            foreach (Transform candidate in candidates)
            {
                if (candidate == null || candidate == keep || candidate.name != BottomNavRootName)
                {
                    continue;
                }

                duplicates.Add(BuildTransformPath(candidate));
            }

            if (duplicates.Count > 0)
            {
                Debug.LogError(
                    "[V0.3-MainHomeRuntimeLock] Duplicate BottomNavBar_Root objects detected; " +
                    "runtime deletion is disabled: " + string.Join(", ", duplicates));
            }
        }

        private static string BuildTransformPath(Transform target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            string path = target.name;
            Transform parent = target.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private static Sprite GetHomeFullBackgroundSprite()
        {
            if (homeFullBackgroundSprite != null)
            {
                return homeFullBackgroundSprite;
            }

            Texture2D texture = Resources.Load<Texture2D>(HomeFullBackgroundResourcePath);
            if (texture == null)
            {
                Debug.LogWarning(
                    $"[V0.3-MainHome] Missing home background resource: {HomeFullBackgroundResourcePath}");
                return null;
            }

            homeFullBackgroundSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            homeFullBackgroundSprite.name = "HomeFullBackground_RuntimeSprite";
            return homeFullBackgroundSprite;
        }

        private void LoadScene(string scenePath, string sceneName, string missingSceneMessage)
        {
            if (SceneUtility.GetBuildIndexByScenePath(scenePath) < 0)
            {
                Debug.LogError(missingSceneMessage, this);
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
